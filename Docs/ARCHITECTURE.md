# Architecture

## Philosophy

The architecture separates data wrangling from business concepts from persistence concerns while keeping the codebase easy to navigate and understand.

---

# Guiding Principles

1. Favor readability over cleverness
2. Prefer explicit code over hidden behavior
3. Build around the graph model
4. Delay abstraction until there is a demonstrated need
5. Keep business concepts independent of Neo4j
6. Optimize only after measuring
7. Isolate external technology details at system boundaries

---

# Layers

## Domain

Defines the business concepts used throughout the application.

Responsibilities:

* Domain records
* Strongly typed identifiers
* Validation result types
* Business vocabulary

Not responsible for:

* Neo4j
* Cypher
* File parsing

---

## Reader

Reads external data sources and converts them into domain records. Reader modules translate source data into the graph domain model.

The structure of the input files does not have to mirror the structure of the domain model. A single source record may produce multiple domain objects if that better represents the business concepts.

For example, a row in `Accounts.tsv` contains both account and ownership information and can therefore produce an `Account` and an `Ownership`.

Responsibilities:

* Parse
* Normalize
* Split one source row into multiple domain objects

Not responsible for:

* Validation
* Neo4j
* Graph persistence
* Cypher

---

## Validation

External data is converted into domain objects before business rules are applied. Validation occurs after parsing and before graph creation.

Each validator has a single responsibility and operates only on domain objects. Readers are responsible for parsing; validators are responsible for business rules; graph modules are responsible for persistence.

This separation keeps parsing, validation, and graph construction independent and testable.

Validation returns valid entities and information about entities or relationships that could not be validated. Validation never modifies domain objects. It either accepts them or rejects them.

Validation errors are represented using domain types rather than formatted strings. The `EntityKey` identifies the entity or relationship involved, while `ValidationIssue` describes the problem.

Validation may accumulate multiple errors for a single entity or relationship when multiple rules fail.

### Current Validation Rules

**Person**

* `PersonId` identifies a Person.
* Identical duplicate records are deduplicated.
* Duplicate `PersonId` records with conflicting attributes are rejected.

**Institution**

* `InstitutionId` identifies an Institution.
* Identical duplicate records are deduplicated.
* Duplicate `InstitutionId` records with conflicting attributes are rejected.

**Customer**

* `CustomerId + InstitutionId` identifies a Customer.
* Identical duplicate records are deduplicated.
* Duplicate Customer keys with conflicting attributes are rejected.
* The referenced Institution must exist among validated Institutions.

**Account**

* `AccountId + InstitutionId` identifies an Account.
* Identical duplicate records are deduplicated.
* Duplicate Account keys with conflicting attributes are rejected.
* The referenced Institution must exist among validated Institutions.

**Ownership**

* The referenced Customer must exist among validated Customers.
* The referenced Account must exist among validated Accounts.
* The Customer and Account must belong to the same Institution.
* Duplicate Ownership records are deduplicated.
* Multiple applicable validation errors may be returned.

### Derived Relationships

`HAS_CUSTOMER_RECORD` and `HELD_AT` do not currently require independent validation modules.

`HAS_CUSTOMER_RECORD` is derived from validated Customer records.

`HELD_AT` is derived from validated Account records. Account validation has already established that the referenced Institution is valid.

---

## Graph

Maps domain records into Neo4j.

```text
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

```text
Infrastructure
├── Neo4j
└── Schema
```

Responsibilities:

* Driver creation
* Sessions
* Transactions
* Database communication
* Neo4j schema constraints

---

## Import

Coordinates the application-level read and validation workflow. `Import.loadAndValidate` calls Reader and Validation modules in dependency order and returns an `ImportResults` aggregate rather than writing directly to the console.

Each imported concept retains both the number of source records read and its typed validation result. This allows the application to report read, valid, and error counts without mixing presentation concerns into Reader or Validation modules.

Responsibilities:

* Coordinate Readers and Validators
* Preserve validation dependency ordering
* Aggregate import results
* Retain source read counts for summary reporting

Not responsible for:

* Neo4j persistence
* Validation rules
* Human-readable validation error formatting

---

## GraphData

Builds graph relationship records that are deterministically derived from validated entities before persistence.

Current derived graph data includes:

* `HAS_CUSTOMER_RECORD` records derived from validated Customers
* `HELD_AT` records derived from validated Accounts

A validated Customer may reference a Person that does not exist. In that case creation of `HAS_CUSTOMER_RECORD` is attempted, but no placeholder Person is created and the Customer remains in the graph.

---

## Reporting

Converts typed validation results into human-readable diagnostic output without adding presentation concerns to the Domain or Validation layers.

`AMLGraph.Reporting.ValidationReport` formats `ValidationError` values and groups multiple validation issues by `EntityKey`. This makes it clear whether several issues belong to one invalid entity or relationship rather than to several separate instances.

The same formatter is available to the application and automated tests. Expecto tests can therefore display useful validation details only when an assertion fails, without temporary print statements in validation code.

Import summary statistics are currently formatted by the application-level `Import` module because `ImportResults` is an application workflow type rather than a Domain type.

---

## Program

Acts as the application composition root and coordinates the high-level workflow. The details of reading, validation, reporting, and derived relationship construction are delegated to their respective modules.

Responsibilities:

* Application startup
* Verify database connectivity and initialize schema
* Invoke `Import.loadAndValidate`
* Present import and validation summaries
* Invoke `GraphData` for derived relationships
* Order graph node and relationship creation

`Program.fs` intentionally remains thin so that the application workflow is readable without containing the implementation details of each stage.

---

# Domain Organization

The domain model is intentionally maintained in a single file (`Domain.fs`).

The project currently contains a small, cohesive domain model, and splitting records into separate files would increase complexity without improving maintainability.

This decision should be revisited only if the domain grows significantly.

## Strongly Typed Domain Identifiers

Entity identifiers are modeled as domain concepts rather than primitive strings. This reduces accidental misuse and keeps entity identity explicit throughout the application.

Basic identifiers include:

```text
PersonId
CustomerId
AccountId
InstitutionId
```

Some identifiers are only unique within the context of an Institution.

Therefore the domain defines compound identifiers:

```text
UniqueCustomerId = CustomerId + InstitutionId
UniqueAccountId  = AccountId + InstitutionId
```

Ownership relationships also have an explicit identity:

```text
OwnershipId = UniqueCustomerId + UniqueAccountId
```

## Entity Identity

The current identity rules are:

```text
Person       → PersonId
Institution  → InstitutionId
Customer     → CustomerId + InstitutionId
Account      → AccountId + InstitutionId
Ownership    → UniqueCustomerId + UniqueAccountId
```

These rules are represented both in the domain model and, where appropriate, by Neo4j schema constraints.

A `CustomerId` by itself is not assumed to be globally unique. The same identifier may exist at multiple Institutions.

Likewise, an `AccountId` by itself is not assumed to be globally unique. The same account identifier may exist at multiple Institutions without referring to the same real-world account.

---

# Person and Customer Modeling

`Person` and `Customer` represent different concepts.

A `Person` represents a real-world individual.

A `Customer` represents an institution-specific customer record.

For example, one Person may be a customer of two financial institutions:

```text
                 Person P001
                    /   \
                   /     \
                  ▼       ▼
          C001 / FI001   C001 / FI002
```

These are two distinct Customer records that refer to the same Person.

This distinction prevents institution-specific customer identifiers and attributes from being incorrectly treated as properties of the real-world Person.

For example, `RiskRating` belongs to Customer rather than Person because different Institutions may assign different risk ratings to the same Person.

A Customer may exist without a corresponding validated Person. The Customer represents an Institution's customer record, while Person represents a resolved real-world individual.

This distinction is important in AML analysis because customer records may contain false, stolen, or otherwise unreliable identity information. An unresolved Person therefore does not invalidate the Customer or its Accounts and may itself represent information relevant to investigation.

---

# Domain Objects Represent Meaning, Not Formatting

Domain types should capture business concepts and rules.

Formatting decisions, including human-readable error messages, belong outside the domain layer.

For example:

Domain:

```text
ConflictingAccountAttributes
```

Presentation:

```text
"Account A100 contains conflicting account attributes."
```

---

# Graph Modeling Conventions

## Current Graph Model

The current graph structure is:

```text
(Person)
    |
    | HAS_CUSTOMER_RECORD
    ▼
(Customer)
    |
    | OWNS
    ▼
(Account)
    |
    | HELD_AT
    ▼
(Institution)
```

A Person may have multiple Customer records.

A Customer may own multiple Accounts.

Multiple Customers may own the same Account.

The same `CustomerId` may represent distinct Customer nodes at different Institutions.

The same `AccountId` may represent distinct Account nodes at different Institutions.

This allows the graph to represent situations such as:

* One Person being a customer of multiple Institutions
* One Customer owning multiple Accounts at an Institution
* Multiple Customers jointly owning an Account
* The same AccountId appearing independently at multiple Institutions

---

## Nodes

Entities that have an independent identity in the business domain are modeled as Neo4j nodes.

Current nodes:

* Person
* Customer
* Account
* Institution

Node modules are responsible for creating and updating node properties.

Examples:

```text
Graph.Nodes.Person.create
Graph.Nodes.Customer.create
Graph.Nodes.Account.create
Graph.Nodes.Institution.create
```

---

## Relationships

Relationships represent meaningful connections between nodes.

Current relationships:

```text
Person   ── HAS_CUSTOMER_RECORD ──> Customer
Customer ── OWNS ─────────────────> Account
Account  ── HELD_AT ──────────────> Institution
```

Relationship modules are responsible for creating relationships between existing nodes.

Examples:

```text
Graph.Relationships.Has_Customer_Record.create
Graph.Relationships.Ownership.create
Graph.Relationships.Held_At.create
```

---

## Properties vs. Nodes

Values should remain properties when they primarily describe an entity or provide evidence about its identity and are not normally traversed as relationships.

Examples of Person properties:

* FirstName
* LastName
* DOB
* Occupation

For example, DOB may be important evidence when determining whether two records refer to the same Person, but sharing a date of birth does not normally represent a meaningful relationship between people.

A value should be considered for its own node when sharing that value creates analytically meaningful connectivity.

Potential future examples include:

```text
Person ── HAS_EMAIL ──> Email
Person ── HAS_PHONE ──> Phone
Person ── RESIDES_AT ──> Address
Person ── USES_DEVICE ──> Device
Person ── WORKS_FOR ──> Organization
```

This distinction keeps graph topology focused on relationships that are useful for traversal and analysis.

---

## Neo4j Identity Constraints

Neo4j constraints enforce the domain's node identity rules.

Conceptually:

```text
Person
    personId

Institution
    institutionId

Customer
    customerId + institutionId

Account
    accountId + institutionId
```

Customer and Account therefore use composite Neo4j node keys.

Schema constraints are maintained in `Infrastructure.Schema`.

During development, schema initialization may remove obsolete AMLGraph constraints before creating the current constraints. This prevents constraints from previous versions of the domain model from incorrectly affecting the current graph.

A production system with persistent data would use controlled schema migrations rather than routinely dropping constraints.

---

# Account Ownership Modeling

Decision:

Customer-to-Account association is modeled as an `OWNS` relationship rather than a `CustomerId` property on Account.

Reason:

An Account may have multiple associated Customers. Modeling ownership as a relationship allows the graph to represent joint ownership and future relationship attributes.

Consequence:

Account nodes represent Accounts independently. Ownership relationships represent Customer associations. The Account does not contain ownership information.

Ownership identifies both endpoints explicitly:

```text
UniqueCustomerId
    CustomerId + InstitutionId

UniqueAccountId
    AccountId + InstitutionId
```

Ownership validation also requires the Customer and Account InstitutionIds to match.

---

# Derived Relationships

Some relationships are explicit domain records because they represent independently validated associations. Others are deterministically derived from validated entities.

## OWNS

`OWNS` is represented by the `Ownership` domain type.

Ownership requires independent validation because its endpoints originate from source data and may reference missing entities or inconsistent Institutions.

## HAS_CUSTOMER_RECORD

`HAS_CUSTOMER_RECORD` connects a Person to an institution-specific Customer record.

The relationship is derived from a validated Customer record using:

```text
PersonId
UniqueCustomerId
```

A separate validation module is not currently required for this relationship.

If the referenced Person does not exist, no placeholder Person is automatically created.

The Customer remains in the graph without the relationship because inability to resolve a Person does not invalidate the Institution's Customer record.

## HELD_AT

`HELD_AT` connects an Account to its Institution.

The relationship is derived from the validated Account:

```text
UniqueAccountId
InstitutionId
```

Account validation has already verified that the Institution exists, so independent `HELD_AT` validation is unnecessary.

---

# Relationship Integrity

Relationships should only connect nodes supported by validated data.

AMLGraph distinguishes between relationships requiring explicit validation and relationships derived from already validated entities.

Explicitly validated:

```text
Customer ── OWNS ──> Account
```

Derived:

```text
Person ── HAS_CUSTOMER_RECORD ──> Customer
Account ── HELD_AT ──> Institution
```

Missing referenced nodes are treated as data-quality issues rather than automatically creating incomplete nodes.

The system does not create generic placeholder nodes from relationship data because doing so could introduce false connectivity between otherwise unrelated entities.

---

# Import Pipeline

`Program.fs` delegates the read and validation portion of the workflow to `Import.loadAndValidate`. The Import module reads foundational entities before dependent entities and preserves the dependency ordering required by validation.

Conceptually:

```text
Program
  ↓
Import.loadAndValidate
  │
  ├─ Read Persons
  │    ↓
  │  Validate Persons
  │
  ├─ Read Institutions
  │    ↓
  │  Validate Institutions
  │
  ├─ Read Customers
  │    ↓
  │  Validate Customers
  │    ↓
  │  requires validated Institutions
  │
  ├─ Read Accounts + Ownerships
  │    ↓
  │  Validate Accounts
  │    ↓
  │  requires validated Institutions
  │
  └─ Validate Ownerships
       ↓
     requires validated Customers + Accounts

ImportResults
  ├─ source read counts
  ├─ validated entities
  └─ validation errors
       ↓
Import summary + ValidationReport
       ↓
GraphData derives HAS_CUSTOMER_RECORD and HELD_AT
       ↓
Create Person Nodes
Create Customer Nodes
Create Institution Nodes
Create Account Nodes
       ↓
Create HAS_CUSTOMER_RECORD Relationships
Create OWNS Relationships
Create HELD_AT Relationships
```

The ordering ensures that dependent validation uses only entities that survived earlier validation. `ImportResults` keeps the original read count beside each validation result so summary statistics such as records read, records validated, and validation errors can be produced after the workflow completes.

---

# Testing

`AMLGraph.Tests` contains automated tests for business rules and domain behavior.

Tests are organized to mirror production concepts.

Current validation test areas include:

```text
Validation
├── Person
├── Customer
├── Institution
├── Account
└── Ownership
```

Validation tests cover both normal behavior and important identity edge cases, including:

* Duplicate identical records
* Conflicting duplicate records
* Missing referenced entities
* Customer and Account Institution mismatches
* Same CustomerId at different Institutions
* Same AccountId at different Institutions
* Multiple Accounts owned by one Customer
* Joint Account ownership
* Multiple validation errors on the same invalid relationship

Derived relationships such as `HAS_CUSTOMER_RECORD` and `HELD_AT` do not currently have independent validation tests because their inputs come directly from validated entities.

## Synthetic Test Data

Test data is maintained separately from production code.

Synthetic test data is used to ensure tests contain no production records or personally identifiable information.

Test data modules use the prefix `Synthetic` to distinguish generated test data from domain entities.

Examples:

```text
SyntheticPerson.john
SyntheticCustomer.john
SyntheticAccount.a100
SyntheticInstitution.bank01
SyntheticOwnership.johnOwnsA100
```

---

# Naming Conventions

Examples:

```text
Reader.Person.read
Reader.Customer.read
Reader.Institution.read
Reader.Account.read

Validation.Person.validate
Validation.Customer.validate
Validation.Institution.validate
Validation.Account.validate
Validation.Ownership.validate

Graph.Nodes.Person.create
Graph.Nodes.Customer.create
Graph.Nodes.Institution.create
Graph.Nodes.Account.create

Graph.Relationships.Has_Customer_Record.create
Graph.Relationships.Ownership.create
Graph.Relationships.Held_At.create
```

Names should reflect domain concepts rather than implementation details.

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

* `Neo4j.verifyConnectionAsync`
* `Neo4j.executeWriteAsync`

Graph modules use async workflows.

Examples:

* `Graph.Nodes.Customer.create`
* `Graph.Relationships.Ownership.create`

`Program.fs` orchestrates workflows using `Async.RunSynchronously`.

Reader modules currently remain synchronous because the application processes one file at a time.

If future requirements involve concurrent file processing or streaming large datasets, asynchronous reader modules may be introduced.

Design rationale:

F# async workflows are used throughout the application because they provide a consistent programming model and keep .NET Task details isolated to the infrastructure layer.

The application should not need to know whether an external dependency uses Task, Async, or another asynchronous abstraction.

---

# Coding Conventions

* Name predicates after business concepts.
* Prefer explicit helper functions over clever pipelines.
* Use `private` for implementation details.
* Public modules expose a single primary function where practical.
* Prefer strongly typed identifiers over primitive strings.
* Keep domain rules out of Reader and Graph modules.
* Derive relationships from validated entities when the relationship contains no independently sourced information.
* Add abstractions only when the domain demonstrates a need for them.
