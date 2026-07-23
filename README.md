# AMLGraph

AMLGraph is a learning project and reference implementation for building Anti-Money Laundering (AML) graph applications using **F#** and **Neo4j**.

The project is intentionally designed to favor **clarity, maintainability, and idiomatic F#** over framework complexity or premature optimization.

AMLGraph is organized around the graph model rather than the data import process.

Its goals are to:

* Learn Neo4j and Cypher through a realistic AML domain.
* Explore graph modeling techniques used in fraud detection and financial crime.
* Demonstrate a clean F# architecture for graph applications.
* Serve as a foundation for experimenting with graph algorithms, entity resolution, and AML analytics.

---

## Current Features

* Read AML data from tab-delimited files
* Validate customer, account, and ownership data
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
├── AMLGraph.slnx
│
├── AMLGraph.App
│   ├── Program.fs
│   ├── Reader
│   ├── Graph
│   └── Infrastructure
│
├── AMLGraph.Domain
│   ├── Domain.fs
│   └── Validation
│       ├── Customer.fs
│       ├── Account.fs
│       └── Ownership.fs
│
├── AMLGraph.SyntheticData
│   └── SyntheticCustomer.fs
│
├── AMLGraph.Tests
│   └── Validation
│       └── Customer.fs
│
└── Docs
```

---

## Data Flow

```
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

---

## Current Status

Completed

* Schema initialization
* Customer node import and validation
* Account and Ownership node import and validation
* Node creation for Customers and Accounts
* Relationship creation for Ownership (Customer)-[:OWNS]->(Account)
* Basic project architecture

Planned

* Transactions
* Transfers
* Entity Resolution
* Graph analytics
* AML investigations

---

# Extension Strategy

New graph concepts should generally require:

1. A new domain record.
1. A reader function.
1. A validation function.
1. A graph node or relationship module.
1. Program orchestration.

The architecture is intended to grow by extension rather than modification.

## Design Goals

The project favors:

* Readability over cleverness
* Explicit code over hidden behavior
* Small, cohesive modules
* Strong separation of concerns
* Incremental development
* Learning through implementation

See **ARCHITECTURE.md** for architectural details and **DECISIONS.md** for the reasoning behind major design choices.
