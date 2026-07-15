# Design Decisions

This document records significant architectural decisions made during the development of AMLGraph.

---

## 2026-07-15

### Domain Organization

**Decision**

Keep all domain records in a single `Domain.fs` file.

**Reason**

The domain model is currently small and cohesive.

Splitting every record into its own file would increase project complexity without providing meaningful benefits.

---

### Graph Organization

**Decision**

Organize graph code using:

```
Graph
├── Nodes
└── Relationships
```

**Reason**

The organization reflects the Neo4j graph model rather than the mechanics of importing data.

---

### Naming Convention

**Decision**

Use module names to identify graph objects and a consistent `create` function for persistence.

Examples:

```fsharp
Graph.Nodes.Customer.create
Graph.Nodes.Account.create
Graph.Relationships.Ownership.create
```

**Reason**

The API is self-describing and remains consistent throughout the project.

---

### Reader Strategy

**Decision**

Readers return domain records.

**Reason**

The domain layer remains independent of Neo4j and can be reused by future applications.

---

### Async Strategy

**Decision**

Neo4j operations are asynchronous.

Readers remain synchronous.

**Reason**

Database communication benefits from asynchronous execution.

Current file processing is sequential and gains little from asynchronous complexity.

---

### Project Philosophy

AMLGraph is intended to be:

* A learning project.
* A reference implementation.
* An example of clean F# architecture.
* A foundation for future AML graph research.
