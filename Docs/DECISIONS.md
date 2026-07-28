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

Reader modules return domain records.

**Reason**

The domain layer remains independent of Neo4j and can be reused by future applications.

---

### Async Strategy

**Decision**

Neo4j operations are asynchronous.

Reader modules remain synchronous.

**Reason**

Database communication benefits from asynchronous execution.

Current file processing is necssarily sequential (customers before accounts) and gains little from asynchronous complexity.

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

When multiple source records share an AccountId, the account is only imported if all account attributes agree except the CustomerId.  Multiple Customers can own the same account, but the other account identifying information must be consistent.

If conflicting account attributes are found (beyond CustomerId), the account is excluded from the graph along with associated ownership relationships.

Rationale:

The system cannot determine which source record is authoritative. Loading one version could create misleading investigative results.

## Validation Errors Use Domain Types

Decision:

Validation errors are represented using domain types rather than string messages.

Examples:
- ValidationIssue is a discriminated union.
- ValidationError contains the affected entity key and issue type.
- Human-readable messages are generated only at the presentation boundary.

Rationale:

Strings describe how an issue is communicated, not what the issue means. Representing validation issues as domain types allows the compiler to enforce consistency and allows reporting, testing, and downstream processing to use the meaning of the error rather than parsing text.

Consequences:

- Validation logic does not contain user-facing messages.
- Reports and logs are responsible for formatting messages.
- New validation scenarios require adding domain cases rather than adding arbitrary strings.

## Strongly Typed Entity Identifiers

Decision:

Entity identifiers are represented using distinct domain types rather than plain strings.

Examples:

```fsharp
type CustomerId = CustomerId of string
type AccountId = AccountId of string
type InstitutionId = InstitutionId of string
```
---

## 2026-07-23

### Synthetic Test Data

Decision:

Test fixtures will use synthetic data rather than copied production-like records.

Example:

```fsharp
CustomerId "SYN-C001"
```

Rationale:

AML applications often process sensitive customer information. Synthetic identifiers and records make it clear that test data is not production data and prevent accidental exposure of PII.

Consequences:

- Test data remains safe to share.
- Developers can recognize synthetic records immediately.
- Test fixtures remain independent from source datasets.

---

### Expecto Testing Framework

Decision:

Use Expecto for automated testing.

Rationale:

Expecto provides a lightweight, idiomatic F# testing framework and integrates well with .NET tooling.

Consequences:

- Tests remain executable F# code.
- Test organization can follow domain concepts.

---

## 2026-07-24

### Domain Library Extraction

Decision:

Domain concepts and validation rules are isolated into AMLGraph.Domain.

Reason:

Multiple consumers require business concepts independently of application orchestration.

---

## 2026-07-25

### Account Key Simplification

Decision:

AccountId is assumed to be globally unique. Real financial systems typically require (InstitutionId, AccountId) to uniquely identify an account

Reason:

This simplification keeps the example focused on graph modeling and validation. The model may be revised when institutions become first-class entities in the graph.

### Currency

Decision:

All monetary values are assumed to be in U.S. dollars (USD). 

Reason:

Currency is intentionally omitted from the domain model to keep the project focused on graph modeling and 
AML concepts. Multi-currency support can be introduced later by modeling monetary values as a Money type.

## 2026-07-28

Decision:

This decision is a reversal.  No longer assuming the AccountId is globally unique.  Account identity = AccountKey = AccountId + InstitutionId
```text
InstitutionId
    → identifies the institution

AccountId
    → identifies an account within that institution

AccountKey
    → identifies the real-world account globally
       (AccountId + InstitutionId)
```

Reason:
It is an oversimplification to use AccountId as a global identifier.