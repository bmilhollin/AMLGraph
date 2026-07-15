# Architecture

## Philosophy

AMLGraph is organized around the graph model rather than the data import process.

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

## Readers

Reads external data sources and converts them into domain records.

Responsibilities:

* File parsing
* Basic validation
* Record creation

Not responsible for:

* Neo4j
* Graph relationships
* Cypher

---

## Graph

Maps domain records into Neo4j.

```
Graph
├── Nodes
└── Relationships
```

Responsibilities:

* Node creation
* Relationship creation
* Cypher
* Parameter mapping

Not responsible for:

* File parsing
* Business rules

---

## Neo4j

Contains infrastructure concerns.

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

# Naming Conventions

## Node modules

```
Graph.Nodes.Customer
Graph.Nodes.Account
Graph.Nodes.Transaction
```

Each exposes a primary `create` function.

Example:

```fsharp
Graph.Nodes.Customer.create customers
```

---

## Relationship modules

```
Graph.Relationships.Ownership
Graph.Relationships.Transfer
```

Each exposes a primary `create` function.

Example:

```fsharp
Graph.Relationships.Ownership.create ownerships
```

---

# Domain Organization

The domain model is intentionally maintained in a single file (`Domain.fs`).

The project currently contains a small, cohesive domain model, and splitting records into separate files would increase complexity without improving maintainability.

This decision should be revisited only if the domain grows significantly.

---

# Async Strategy

Neo4j operations are asynchronous because they involve network I/O.

Readers currently remain synchronous because the application processes one file at a time.

If future requirements involve concurrent file processing or streaming large datasets, asynchronous readers may be introduced.

# Async v. Task Strategy

AMLGraph uses F# async workflows at the application and graph layers.

The Neo4j .NET driver is task-based, so the Infrastructure layer is responsible for converting Task-based APIs into F# Async workflows.

## Async Boundary

The dependency flow is:

Neo4j Driver
    |
    | Task
    ▼
Infrastructure.Neo4j
    |
    | Async
    ▼
Graph / Program


Infrastructure functions expose Async results:

- Neo4j.verifyConnectionAsync
- Neo4j.executeWriteAsync

Graph modules use async workflows:

- Graph.Nodes.Customer.create
- Graph.Nodes.Account.create
- Graph.Relationships.Ownership.create

Program.fs orchestrates workflows using Async.RunSynchronously.

## Design Rationale

F# async workflows are used throughout the application because they provide a consistent programming model and keep .NET Task details isolated to the infrastructure layer.

The application should not need to know whether an external dependency uses Task, Async, or another asynchronous abstraction.

---

# Extension Strategy

New graph concepts should generally require:

1. A new domain record.
2. A reader function.
3. A graph node or relationship module.
4. Program orchestration.

The architecture is intended to grow by extension rather than modification.
