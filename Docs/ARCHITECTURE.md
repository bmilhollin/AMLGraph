# Architecture

## Philosophy

The architecture separates business concepts from persistence concerns while keeping the codebase easy to navigate and understand.

---

# Guiding Principles

1. Favor readability over cleverness.
2. Prefer explicit code over hidden behavior.
3. Build around the graph model.
4. Delay abstraction until there is a demonstrated need.
5. Keep business concepts independent of Neo4j.
6. Keep modules cohesive.
7. Optimize only after measuring.
8. Isolate external technology details at system boundaries.

---

# Layers

## Domain

Defines the business concepts used throughout the application.

Responsibilities:

* Domain records
* Business vocabulary

Not responsible for:

* Neo4j
* Cypher
* File parsing

---

## Reader

Reads external data sources and converts them into domain records.  Reader modules translate source data into the graph domain model.

The structure of the input files does not have to mirror the structure of the domain model. A single source record may produce multiple domain objects if that better represents the business concepts.

Responsibilities:

* Parse
* Normalize
* Split one source row into multiple domain objects

Not responsible for:

* Validate
* Neo4j
* Graph relationships
* Cypher

---

## Validation

External data is converted into domain objects before any business rules are applied.  Validation occurs after parsing and before graph creation.

Each validator has a single responsibility and operates only on domain objects. Each reader are responsible for parsing; validators are responsible for business rules; graph modules are responsible for persistence.  This separation keeps parsing, validation, and graph construction independent and testable.

Validation returns a validated entity and information on entities that could not be validated. Validation never modifies domain objects. It either accepts them or rejects them.

---

## Graph

Maps domain records into Neo4j.

```
Graph
├── Nodes
└── Relationships
```

Responsibilities:

* Parameter mapping
* Cypher
* Node creation
* Relationship creation

Not responsible for:

* File parsing
* Business rules

---

## Infrastructure

Contains Neo4j infrastructure concerns.

```
Infrastructure
├── Neo4j
└── Schema
```

Responsibilities:

* Driver creation
* Sessions
* Transactions
* Database communication

---

## Program

Coordinates the application workflow.

Responsibilities:

* Application startup
* Orchestration
* Ordering of operations

---

# Testing

AMLGraph.Tests contains automated tests for business rules and domain behavior.

Tests are organized to mirror production concepts.

Example:
```text
Validation
├── Customer
├── Account
└── Ownership
```

Test data is maintained separately from production code.

Synthetic test data is used to ensure tests contain no production records or personally identifiable information.

# Naming Conventions

Examples
- Reader.Customer.read
- Validation.Customer.validate
- Graph.Nodes.Customer.create

## Synthetic Test Data

Test data modules use the prefix `Synthetic` to distinguish generated test data from domain entities.  Synthetic data is intentionally created for testing and contains no production records.

Examples
- SyntheticCustomer.john
- SyntheticAccount.checkingAccount

# Domain Organization

The domain model is intentionally maintained in a single file (`Domain.fs`).

The project currently contains a small, cohesive domain model, and splitting records into separate files would increase complexity without improving maintainability.

This decision should be revisited only if the domain grows significantly.

## Strongly Typed Domain Identifiers

Entity identifiers are modeled as domain concepts rather than primitive strings. This reduces accidental misuse and keeps entity identity explicit throughout the application.

## Domain Objects Represent Meaning, Not Formatting

Domain types should capture business concepts and rules.

Formatting decisions, including human-readable error messages, belong outside the domain layer.

For example:

Domain:
    ConflictingAccountAttributes

Presentation:
    "Account A100 contains conflicting account attributes."

---

# Async Strategy

AMLGraph uses F# async workflows at the application and graph layers.  
The Neo4j .NET driver is task-based, so the Infrastructure layer is responsible for converting Task-based APIs into F# Async workflows.

The dependency flow is:


```text
Neo4j Driver
    |
    | Task
    ▼
Infrastructure.Neo4j
    |
    | Async
    ▼
Graph / Program
```

Infrastructure functions expose Async results:

- Neo4j.verifyConnectionAsync
- Neo4j.executeWriteAsync

Graph modules use async workflows:

- Graph.Nodes.Customer.create
- Graph.Relationships.Ownership.create

Program.fs orchestrates workflows using Async.RunSynchronously.

Reader modules currently remain synchronous because the application processes one file at a time. 
If future requirements involve concurrent file processing or streaming large datasets, asynchronous reader modules may be introduced.

Design Rationale:

F# async workflows are used throughout the application because they provide a consistent programming model and keep .NET Task details isolated to the infrastructure layer.

The application should not need to know whether an external dependency uses Task, Async, or another asynchronous abstraction.

---

# Graph Modeling Conventions

## Nodes

Entities that have an independent identity in the business domain are modeled as Neo4j nodes.

Examples:

- Customer
- Account
- Transaction

Node modules are responsible for creating and updating node properties.

Example:

Graph.Nodes.Customer.create

## Relationships

Relationships represent meaningful connections between nodes.

Examples:

- Customer OWNS Account
- Account SENDS Transaction
- Customer ASSOCIATED_WITH Entity

Relationship modules are responsible for creating relationships between existing nodes.

Example:

Graph.Relationships.Ownership.create

## Identity

Nodes should have a stable business identifier.

Example:

Customer.customerId
Account.accountId

Neo4j constraints should enforce uniqueness for node identities.  These contraints are found in Infrastructure.Schema.

## Account Ownership Modeling

Decision:

Customer-to-Account association will be modeled as an Ownership relationship rather than a CustomerId property on Account.

Reason:

An account may have multiple associated customers. Modeling ownership as a relationship allows the graph to represent joint ownership and future relationship attributes.

Consequence:

Account nodes represent accounts independently. Ownership relationships represent customer associations.  The Account does not contain any ownership information.

## Relationship Integrity

Relationships are created only between existing nodes.

The import workflow creates nodes before relationships:

1. Load and validate Customer nodes
2. Load and validate Account nodes
3. Validate relationship references (do the nodes exist? did they survive validation?)
4. Create relationships

Missing referenced nodes are treated as data quality issues rather than automatically creating incomplete nodes.

The system should not create placeholder nodes from relationship data alone because this can introduce entities with insufficient supporting information.

## Import Pipeline

```text
Read Customers
      ↓
Validate Customers
      ↓
Read Accounts + Ownerships
      ↓
Validate Accounts
      ↓
Validate Ownerships
      ↓
Create Customer Nodes
      ↓
Create Account Nodes
      ↓
Create Ownership Relationships
```

# Coding Conventions

* Name predicates after business concepts.
* Prefer explicit helper functions over clever pipelines.
* Use private for implementation details.
* Public modules expose a single primary function where practical.


