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

### Async Abstraction Boundary

Decision:

Use F# Async workflows above the Infrastructure layer and isolate .NET Tasks inside Infrastructure.

Context:

The Neo4j .NET driver exposes Task-based asynchronous APIs. Mixing Task and Async throughout the application would require every caller to understand the underlying database library.

Reason:

The application code should depend on application concepts, not implementation details of external libraries.

Consequences:

- Infrastructure modules convert Task to Async using Async.AwaitTask.
- Graph modules expose Async workflows.
- Program.fs coordinates Async workflows.
- Future infrastructure libraries should follow the same pattern.

## 2026-07-16

### Relationship Creation Requires Existing Nodes

Decision:

Relationship imports will validate that referenced nodes exist before creating Neo4j relationships.

Context:

Graph relationships depend on existing nodes. Ownership data may reference customers or accounts that are missing from the source data or failed validation during import.

Alternatives considered:

1. Silently ignore missing nodes.
2. Automatically create missing nodes using only identifiers.
3. Validate references and report data quality issues.

Decision rationale:

Automatically creating nodes from incomplete relationship data can introduce incomplete or misleading entities into the graph. Silently ignoring relationships hides data quality problems.

The import process will validate references and report missing entities.

Consequences:

- Nodes must be loaded before relationships.
- Relationship imports can identify missing references.
- Data quality issues are visible rather than silently lost.

## Account Identity Conflicts

Decision:

When multiple source records share an AccountId, the account is only imported if all account attributes agree.

If conflicting account attributes are found, the account is excluded from the graph along with associated ownership relationships.

Rationale:

The system cannot determine which source record is authoritative. Loading one version could create misleading investigative results.