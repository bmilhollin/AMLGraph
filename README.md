# AMLGraph

> Building Anti-Money Laundering graph applications with F#, Neo4j, and a domain-driven architecture.

<p align="center">
    <img src="Docs/images/neo4j.png" width="850">
</p>

AMLGraph is a learning project and reference implementation for building Anti-Money Laundering (AML) graph applications using F# and Neo4j. The project emphasizes clean architecture, domain modeling, and graph-based analysis over framework complexity or premature optimization.

## Why this project?

Many Neo4j examples focus on Cypher queries or graph algorithms.

AMLGraph instead explores how to design a graph application from the ground up using clean domain modeling, validation, and explicit architectural boundaries. The project intentionally favors readability and maintainability over framework magic.

## Goals

* Learn Neo4j and Cypher through a realistic AML domain
* Explore graph modeling techniques used in fraud detection and financial crime
* Demonstrate a clean F# architecture for graph applications
* Serve as a foundation for experimenting with graph algorithms, entity resolution, and AML analytics

##Current Status

```text
✔ Customer import
✔ Account import
✔ Validation
✔ Synthetic data Library
✔ Expecto test suite
◻ Transactions
◻ Entity resolution
◻ Graph analytics
◻ AML investigations
```

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
│   └── SyntheticAccount.fs
│   └── SyntheticOwnership.fs
│
├── AMLGraph.Tests
│   └── Validation
│       └── Customer.fs
│       └── Accounts.fs
│       └── Ownerships.fs
│
└── Docs
```

---

## Import Pipeline

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

# Extension Strategy

New graph concepts should generally require:

1. A new domain record
1. A reader function
1. A validation function
1. A graph node or relationship module
1. Program orchestration

The architecture is intended to grow by extension rather than modification.

See **ARCHITECTURE.md** for architectural details and **DECISIONS.md** for the reasoning behind major design choices.
