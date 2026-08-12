# Design Decisions

This document records significant architectural and domain-modeling decisions made during the development of AMLGraph.

Where a later decision reverses or refines an earlier one, the earlier decision is retained for historical context and marked as superseded.

---

## 2026-07-15

### Domain Organization

**Decision**

Keep all domain records in a single `Domain.fs` file.

**Reason**

The domain model is currently small and cohesive.

Splitting every record into its own file would increase project complexity without providing meaningful benefits.

**Consequence**

This decision should be revisited only if the domain grows enough that a single file becomes difficult to navigate or maintain.

---

### Graph Organization

**Decision**

Organize graph code using:

```text
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

A source row may produce more than one domain object if that better reflects the business model.

---

### Async Strategy

**Decision**

Neo4j operations are asynchronous.

Reader modules remain synchronous.

**Reason**

Database communication benefits from asynchronous execution.

Current file processing is sequential and gains little from asynchronous reader complexity.

---

### Project Philosophy

AMLGraph is intended to be:

* A learning project
* A reference implementation
* An example of clean F# architecture
* A foundation for future AML graph research

The project favors readability, explicit domain modeling, and clear architectural boundaries over framework complexity or premature abstraction.

---

### Async Abstraction Boundary

**Decision**

Use F# Async workflows above the Infrastructure layer and isolate .NET Tasks inside Infrastructure.

**Context**

The Neo4j .NET driver exposes Task-based asynchronous APIs. Mixing Task and Async throughout the application would require every caller to understand the underlying database library.

**Reason**

Application code should depend on application concepts rather than implementation details of external libraries.

**Consequences**

* Infrastructure modules convert Task to Async using `Async.AwaitTask`
* Graph modules expose Async workflows
* `Program.fs` coordinates Async workflows
* Future infrastructure libraries should follow the same pattern where practical

---

## 2026-07-16

### Relationship Creation Requires Existing Nodes

**Decision**

Relationship imports validate that referenced nodes exist before relationships are accepted.

**Context**

Graph relationships depend on existing nodes. Ownership data may reference Customers or Accounts that are missing from source data or failed validation.

**Alternatives considered**

1. Silently ignore missing nodes
2. Automatically create missing nodes using only identifiers
3. Validate references and report data-quality issues

**Decision rationale**

Automatically creating nodes from incomplete relationship data can introduce incomplete or misleading entities into the graph.

Silently ignoring relationships hides data-quality problems.

The import process therefore validates relationship endpoints and reports missing entities.

**Consequences**

* Nodes must be loaded before relationships
* Relationship imports can identify missing references
* Data-quality issues remain visible
* Placeholder nodes are not created automatically from incomplete relationship data

---

### Account Attribute Conflicts

**Decision**

When multiple source records refer to the same Account identity, the Account is imported only if its Account attributes agree.

Multiple Customers may own the same Account, but non-ownership Account attributes must remain consistent.

**Rationale**

The system cannot determine which conflicting source record is authoritative. Loading one version could create misleading investigative results.

**Consequence**

Conflicted Accounts are excluded from the graph and reported through validation errors.

---

### Validation Errors Use Domain Types

**Decision**

Validation errors are represented using domain types rather than string messages.

Examples:

* `ValidationIssue` is a discriminated union
* `ValidationError` contains the affected entity key and issue type
* Human-readable messages are generated only at the presentation boundary

**Rationale**

Strings describe how an issue is communicated, not what the issue means.

Representing validation issues as domain types allows the compiler to enforce consistency and allows reporting, testing, and downstream processing to use the meaning of the error rather than parsing text.

**Consequences**

* Validation logic does not contain user-facing messages
* Reports and logs are responsible for formatting messages
* New validation scenarios require adding domain cases rather than arbitrary strings

---

### Strongly Typed Entity Identifiers

**Decision**

Entity identifiers are represented using distinct domain types rather than plain strings.

Examples:

```fsharp
type PersonId = PersonId of string
type CustomerId = CustomerId of string
type AccountId = AccountId of string
type InstitutionId = InstitutionId of string
```

**Reason**

Strongly typed identifiers reduce accidental misuse and make entity identity explicit throughout the application.

---

## 2026-07-23

### Synthetic Test Data

**Decision**

Test fixtures use synthetic data rather than copied production-like records.

Example:

```fsharp
CustomerId "SYN-C001"
```

**Rationale**

AML applications often process sensitive customer information.

Synthetic identifiers and records make it clear that test data is not production data and reduce the risk of accidental exposure of personally identifiable information.

**Consequences**

* Test data remains safe to share
* Developers can recognize synthetic records immediately
* Test fixtures remain independent from source datasets

---

### Expecto Testing Framework

**Decision**

Use Expecto for automated testing.

**Rationale**

Expecto provides a lightweight, idiomatic F# testing framework and integrates well with .NET tooling.

**Consequences**

* Tests remain executable F# code
* Test organization can follow domain concepts
* Validation behavior is verified independently from Neo4j persistence

---

## 2026-07-24

### Domain Library Extraction

**Decision**

Domain concepts and validation rules are isolated into `AMLGraph.Domain`.

**Reason**

Multiple consumers may require business concepts independently of application orchestration.

**Consequence**

The Domain library contains no Neo4j persistence concerns.

---

## 2026-07-25

### AccountId Assumed Globally Unique

**Status**

Superseded.

**Original decision**

Assume `AccountId` is globally unique.

**Original reason**

This simplification kept the initial example focused on graph modeling and validation.

**Superseded by**

The later decision to model Account identity as `AccountId + InstitutionId`.

---

### Currency

**Decision**

All monetary values are currently assumed to be in U.S. dollars.

**Reason**

Currency is intentionally omitted from the domain model to keep the project focused on graph modeling and AML concepts.

**Future extension**

Multi-currency support may later introduce a `Money` type containing amount and currency.

---

## 2026-07-28

### Account Identity Is Institution-Scoped

**Decision**

Do not treat `AccountId` as globally unique.

An Account is uniquely identified by:

```text
AccountId + InstitutionId
```

represented in the domain as:

```fsharp
type UniqueAccountId =
    UniqueAccountId of AccountId * InstitutionId
```

**Reason**

Account identifiers are typically assigned within an institution. The same `AccountId` may legitimately exist at more than one financial institution.

**Identity model**

```text
InstitutionId
    → identifies the institution

AccountId
    → identifies an account within that institution

UniqueAccountId
    → identifies the account within AMLGraph
       using AccountId + InstitutionId
```

**Consequences**

* Account validation groups by `UniqueAccountId`
* Ownership references `UniqueAccountId`
* Neo4j matches Accounts using both `accountId` and `institutionId`
* Neo4j uses a composite Account node key
* The same `AccountId` may exist at different Institutions as separate nodes

---

### Account Key Is a Named Domain Type

**Decision**

Represent compound Account identity with a named type rather than passing a raw tuple throughout the application.

```fsharp
type UniqueAccountId =
    UniqueAccountId of AccountId * InstitutionId
```

**Reason**

The named type expresses domain meaning and prevents callers from treating arbitrary `(AccountId * InstitutionId)` tuples as interchangeable values.

**Consequence**

`Account` exposes its compound identity through a calculated member:

```fsharp
account.Key
```

---

## 2026-08-08

### Institution Is a First-Class Node

**Decision**

Model financial institutions as `Institution` nodes.

Initial Institution attributes are:

```text
InstitutionId
Name
InstitutionType
CountryCode
```

**Reason**

Institution is an independent business entity and is required to establish Customer and Account identity.

`CountryCode` is included because jurisdiction is relevant to future sanctions and AML analysis.

**Consequences**

* Institutions are loaded and validated independently
* Customer and Account validation depend on validated Institutions
* Neo4j enforces uniqueness on `institutionId`

---

### Two-Letter Country Codes

**Decision**

Use a two-letter `CountryCode` field for Institution.

**Reason**

A normalized country code is more useful for AML and sanctions analysis than unrestricted country-name strings.

**Current scope**

Reader validation currently enforces the two-character structure.

Full ISO 3166-1 alpha-2 validation may be added later if needed.

---

### Account-to-Institution Relationship

**Decision**

Represent the relationship between Account and Institution as:

```text
Account ── HELD_AT ──> Institution
```

rather than using the more generic relationship name `AT`.

**Reason**

`HELD_AT` communicates the business meaning of the relationship directly and makes Cypher traversals easier to understand.

---

### HELD_AT Is Derived From Valid Accounts

**Decision**

Do not read `HELD_AT` as a separate source record.

Derive the relationship from validated Accounts.

**Reason**

A validated Account already contains both values required to identify the relationship:

```text
UniqueAccountId
InstitutionId
```

There is no independently sourced information in `HELD_AT`.

**Consequence**

`HELD_AT` does not currently require a separate validation module.

---

### Customer Identity Is Institution-Scoped

**Decision**

Do not treat `CustomerId` as globally unique.

A Customer is uniquely identified by:

```text
CustomerId + InstitutionId
```

represented as:

```fsharp
type UniqueCustomerId =
    UniqueCustomerId of CustomerId * InstitutionId
```

**Reason**

Customer identifiers are institution-specific in the same way that Account identifiers are institution-specific.

The same `CustomerId` may legitimately exist at multiple Institutions, and the same real-world person may have different CustomerIds at different Institutions.

**Consequences**

* Customer validation groups by `UniqueCustomerId`
* Customer Neo4j nodes use a composite key
* Ownership references `UniqueCustomerId`
* Neo4j matches Customers using both `customerId` and `institutionId`
* The same CustomerId may exist at different Institutions as distinct Customer nodes

---

### Customer Constraints Mirror Customer Identity

**Decision**

Neo4j enforces Customer identity using:

```text
customerId + institutionId
```

rather than `customerId` alone.

**Reason**

A single-property Customer constraint incorrectly collapses institution-specific Customer records.

**Consequence**

The Customer schema constraint uses a composite node key.

This decision was prompted by a graph defect in which Customer nodes with the same `customerId` at different Institutions were being merged, causing valid `OWNS` relationships to disappear.

---

### Development Schema Initialization May Replace Known Constraints

**Decision**

During development, schema initialization may explicitly drop known AMLGraph constraints before recreating the current intended constraints.

**Reason**

Domain identity rules are still evolving, and stale Neo4j constraints can continue enforcing superseded assumptions.

**Consequence**

Development schema initialization is deterministic relative to the current code.

A production implementation with persistent data would use controlled schema migrations rather than routinely dropping constraints.

---

## 2026-08-09

### Person and Customer Are Separate Domain Concepts

**Decision**

Separate real-world identity from institution-specific customer records.

```text
Person
    → real-world individual

Customer
    → institution-specific customer record
```

**Reason**

A CustomerId is institution-dependent and should not be treated as a global person identifier.

A single real-world person may have multiple Customer records across financial institutions.

**Consequences**

* Personal descriptive data moves from Customer to Person
* Customer contains institution-scoped information
* Person becomes a Neo4j node
* Customer remains a separate Neo4j node
* One Person may connect to multiple Customer nodes

---

### Use Person Rather Than Entity

**Decision**

Use the domain name `Person` for real-world individuals rather than `Entity`.

**Reason**

The codebase already contains concepts such as `EntityIds` and `EntityKey`.

Using `Entity` for a node type would create unnecessary naming ambiguity.

`Person` is also more precise for the current scope.

**Future consideration**

If AMLGraph later models organizations and other non-person parties, a broader abstraction such as `Party` may be introduced.

---

### Person Starter Properties

**Decision**

The initial Person model contains:

```text
PersonId
FirstName
LastName
Dob
Occupation
```

**Reason**

These fields provide a useful starter set of descriptive and identity-supporting information without over-expanding the model.

**Consequence**

Additional identity evidence such as national identifiers, email, address, phone, or devices may be modeled later as requirements emerge.

---

### Risk Rating Belongs to Customer

**Decision**

Keep `RiskRating` on Customer rather than Person.

**Reason**

Risk rating is an institution-specific assessment.

The same real-world Person may have different risk ratings at different Institutions.

---

### PersonId Is Not Part of Customer Identity

**Decision**

Customer identity remains:

```text
CustomerId + InstitutionId
```

`PersonId` is a reference from Customer to Person, not part of the Customer key.

**Reason**

A Customer record represents information held by a financial institution and may remain valid institution data even when the referenced Person cannot be resolved or validated.

In an AML context, this distinction is important. A customer or account may have been created using false, stolen, or otherwise unreliable identity information. Rejecting the Customer because no valid Person exists would discard potentially significant investigative information.

Person resolution and Customer validity are therefore separate concerns.

**Consequence**

A missing Person does not automatically invalidate the Customer record.

---

### HAS_CUSTOMER_RECORD Relationship

**Decision**

Represent the Person-to-Customer relationship as:

```text
Person ── HAS_CUSTOMER_RECORD ──> Customer
```

**Reason**

The relationship explicitly distinguishes the real-world Person from the institution-specific Customer record.

---

### HAS_CUSTOMER_RECORD Is Derived

**Decision**

Derive `HAS_CUSTOMER_RECORD` from validated Customer records rather than reading it as separate source data.

**Reason**

The validated Customer already contains the necessary relationship endpoints:

```text
PersonId
UniqueCustomerId
```

**Consequence**

`HAS_CUSTOMER_RECORD` does not currently require a separate validation module.

---

### Explicit Relationship Types

**Decision**

Represent graph relationships with explicit domain types when doing so improves clarity.

Examples include:

```text
Ownership
Held_At
Has_Customer_Record
```

**Reason**

Explicit relationship types make the domain vocabulary visible, allow the graph layer to depend only on required endpoint information, and leave room for future relationship-specific properties or behavior.

**Consequence**

Relationship types should remain minimal unless the relationship itself gains independently meaningful attributes.

---

### No Generic Missing-Person Node

**Decision**

Do not create a single generic placeholder Person node for unresolved Person references.

**Reason**

Connecting multiple Customers to a shared “Missing Person” node would introduce a false graph relationship between unrelated customers.

**Consequence**

An unresolved Person reference may result in a Customer node without a `HAS_CUSTOMER_RECORD` relationship.

A future distinct placeholder per unresolved `PersonId` may be considered only if a real workflow requires it.

---

### Properties Versus Nodes

**Decision**

Use properties for information that primarily describes or helps identify an entity.

Use nodes when shared values create analytically meaningful relationships.

**Examples**

Person properties:

```text
FirstName
LastName
Dob
Occupation
```

Potential future nodes:

```text
Email
Phone
Address
Device
Organization
```

**Reason**

Two people sharing a DOB is usually useful as entity-resolution evidence, not as a graph relationship.

Two people sharing an email, device, address, or organization may represent meaningful connectivity worth traversing.

---

## 2026-08-10

### Ownership Uses Full Compound Endpoint Identities

**Decision**

Ownership references:

```text
UniqueCustomerId
UniqueAccountId
```

rather than simple `CustomerId` and `AccountId`.

**Reason**

Both Customer and Account identities are institution-scoped.

Using incomplete identifiers would make Ownership ambiguous when the same CustomerId or AccountId exists at multiple Institutions.

---

### Ownership Identity

**Decision**

Represent an Ownership relationship identity as:

```fsharp
type OwnershipId =
    OwnershipId of UniqueCustomerId * UniqueAccountId
```

**Reason**

Validation errors involving the relationship itself should identify the relationship rather than incorrectly blaming one valid endpoint.

**Consequence**

`EntityKey` includes:

```fsharp
OwnershipKey of OwnershipId
```

---

### Ownership Institution Consistency

**Decision**

An Ownership relationship is valid only when the Customer and Account belong to the same Institution.

**Reason**

The source model describes ownership of an Account within an institution-specific customer relationship.

A Customer at FI001 should not be linked to an Account at FI002 through the same Ownership record.

**Validation issue**

```text
MismatchedInstitutions
```

---

### Validation Accumulates Independent Errors

**Decision**

When multiple independent Ownership validation rules fail, return all applicable errors rather than only the first error.

Examples may include:

```text
MissingCustomer
MissingAccount
MismatchedInstitutions
```

**Reason**

Validation should provide complete data-quality information for a source relationship rather than requiring repeated repair cycles to discover one issue at a time.

**Consequence**

Ownership validation independently evaluates each rule and collects applicable validation errors.

---

## 2026-08-12

### Customer Validation Requires a Valid Institution

**Decision**

Customer validation requires `InstitutionId` to exist in the set of validated Institutions.

**Reason**

InstitutionId is part of `UniqueCustomerId`.

A Customer cannot have a trustworthy institution-scoped identity if the referenced Institution itself is missing or conflicted.

**Consequence**

Customer validation may return:

```text
MissingInstitution
```

and the Customer is excluded from the valid Customer set.

---

### Account Validation Requires a Valid Institution

**Decision**

Account validation requires `InstitutionId` to exist in the set of validated Institutions.

**Reason**

InstitutionId is part of `UniqueAccountId`.

A valid Account identity therefore depends on a valid Institution.

---

### Validation Coverage for the Core Model

**Decision**

Maintain automated validation tests for:

```text
Person
Customer
Institution
Account
Ownership
```

Derived relationships do not currently require separate validation suites:

```text
HAS_CUSTOMER_RECORD
HELD_AT
```

**Reason**

`HAS_CUSTOMER_RECORD` is derived from validated Customers.

`HELD_AT` is derived from validated Accounts whose Institutions have already been validated.

**Current core validation expectations**

* Identical duplicate node records are deduplicated
* Conflicting duplicate node records are rejected
* Customer and Account identities are institution-scoped
* Customer and Account references to Institutions are validated
* Ownership validates both endpoints
* Ownership validates Institution consistency
* Validation may accumulate multiple applicable errors

---

### Import Workflow Is Encapsulated in the Import Module

**Decision**

Keep `Program.fs` focused on application orchestration and move the read-and-validate workflow into `Import.loadAndValidate`.

`Import.loadAndValidate` reads source files, validates entities in dependency order, and returns an `ImportResults` value.

**Reason**

As additional domain concepts and validation rules were added, `Program.fs` became responsible for too many low-level workflow details. The read-and-validate sequence is a cohesive application concern and can be expressed more clearly behind a single import function.

Keeping this logic in `Import` allows `Program.fs` to describe the high-level application flow rather than the mechanics of each import step.

**Consequences**

* `Program.fs` coordinates connection setup, schema initialization, import, reporting, graph creation, and shutdown.
* `Import` coordinates Readers and Validators in the required dependency order.
* `ImportResult<'T>` retains both the number of source records read and the corresponding `Validated<'T list>` result.
* `ImportResults` aggregates the results for Person, Institution, Customer, Account, and Ownership imports.
* Validation errors can be collected from the aggregate result without duplicating that logic in `Program.fs`.
* Reader and Validation modules remain responsible only for parsing and business-rule validation respectively.

---

### Validation Reporting Is Separate From Validation Logic

**Decision**

Keep validation issues as typed domain values and format them for human consumption in `AMLGraph.Reporting.ValidationReport`.

Validation reporting groups errors by `EntityKey` so multiple issues associated with one entity or relationship are presented together.

**Reason**

Validation modules should describe what is wrong, not how the problem is displayed. Human-readable formatting is a presentation concern and should remain outside the Domain and Validation layers.

Grouping by `EntityKey` also makes it clear whether several validation issues belong to one invalid instance or to several different instances. This is especially important for relationships such as Ownership, where one source record may produce multiple independent validation issues.

**Consequences**

* Validators continue to return `ValidationError` values containing domain-level `EntityKey` and `ValidationIssue` values.
* `ValidationReport` converts identifiers and issues into readable text.
* Multiple issues for the same entity or relationship are reported as one grouped block.
* Expecto tests can reuse the same formatter in assertion failure messages instead of adding temporary `printfn` statements.
* Reporting can later write to console, file, or another destination without changing validation behavior.

---

### Derived Graph Data Is Constructed Outside Program

**Decision**

Construct deterministically derived relationship records in `GraphData` rather than directly in `Program.fs`.

Current derived graph data includes:

```text
HAS_CUSTOMER_RECORD
HELD_AT
```

**Reason**

The relationship records are derived from validated domain entities and do not represent application orchestration. Moving their construction out of `Program.fs` keeps the composition root focused on sequencing while keeping graph-specific preparation explicit and reusable.

**Consequences**

* `GraphData.hasCustomerRecords` derives Person-to-Customer relationship records from validated Customers.
* `GraphData.heldAts` derives Account-to-Institution relationship records from validated Accounts.
* The documented rule remains in force that an unresolved Person does not invalidate a Customer; a derived `HAS_CUSTOMER_RECORD` may therefore fail to match a Person without removing the Customer from the graph.

---

# Current Core Identity Model

The current AMLGraph identity rules are:

```text
Person
    PersonId

Institution
    InstitutionId

Customer
    CustomerId + InstitutionId
    = UniqueCustomerId

Account
    AccountId + InstitutionId
    = UniqueAccountId

Ownership
    UniqueCustomerId + UniqueAccountId
    = OwnershipId
```

These identity rules are enforced consistently across:

* Domain types
* Validation
* Synthetic test data
* Neo4j node matching
* Neo4j schema constraints
* Relationship creation

---

# Current Core Graph Model

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

The model supports:

* One Person with Customer records at multiple Institutions
* The same CustomerId at multiple Institutions
* One Customer owning multiple Accounts
* Multiple Customers jointly owning one Account
* The same AccountId at multiple Institutions
* Explicit institution-scoped identity throughout the graph
