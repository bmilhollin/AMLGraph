# AMLGraph

AMLGraph is a learning project and reference implementation for building Anti-Money Laundering (AML) graph applications using **F#** and **Neo4j**.

The project is intentionally designed to favor **clarity, maintainability, and idiomatic F#** over framework complexity or premature optimization.

Its goals are to:

* Learn Neo4j and Cypher through a realistic AML domain.
* Explore graph modeling techniques used in fraud detection and financial crime.
* Demonstrate a clean F# architecture for graph applications.
* Serve as a foundation for experimenting with graph algorithms, entity resolution, and AML analytics.

---

## Current Features

* Read AML data from tab-delimited files
* Create Neo4j nodes using `MERGE`
* Enforce uniqueness with Neo4j constraints
* Organize graph logic separately from business logic
* Idempotent imports (running the importer multiple times does not create duplicate nodes)

---

## Technology Stack

* F#
* .NET
* Neo4j
* Cypher
* Neo4j .NET Driver

---

## Project Structure

```text
AMLGraph
│
├── Domain
├── Readers
├── Graph
│   ├── Nodes
│   └── Relationships
├── Neo4j
├── Schema
└── Program
```

---

## Data Flow

```
Delimited Files
      │
      ▼
Readers
      │
      ▼
Domain Records
      │
      ▼
Graph Nodes / Relationships
      │
      ▼
Neo4j
```

---

## Current Status

Completed

* Customer node import
* Schema initialization
* Duplicate prevention
* Basic project architecture

Planned

* Account nodes
* Customer ownership relationships
* Transactions
* Transfers
* Entity Resolution
* Graph analytics
* AML investigations

---

## Design Goals

The project favors:

* Readability over cleverness
* Explicit code over hidden behavior
* Small, cohesive modules
* Strong separation of concerns
* Incremental development
* Learning through implementation

See **ARCHITECTURE.md** for architectural details and **DECISIONS.md** for the reasoning behind major design choices.
