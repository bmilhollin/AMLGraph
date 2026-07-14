This project is a result of reading this article.
https://www.drivewingrow.com/how-to-build-a-simple-fraud-alert-system-using-neo4j/

All data is fabricated, does not represent real entities

Nodes
(:Customer)
    CustomerId (key is required and unique)
    FirstName
    LastName
    DOB
    Occupation
    RiskRating
(:Account)
    AccountId (key)
    CustomerId
    InstitutionId
    AccountType
    OpenDate
    Balance
(:FinancialInstitution)
    institutionId (key)
    name
    institutionType
    country
(:Transaction)
(:Device)
(:IPAddress)
(:Merchant)
(:Email)
(:Phone)
(:Address)
(:Country)

Relationships
(Customer)-[:OWNS]->(Account)

(Account)-[:SENT]->(Transaction)

(Transaction)-[:TO]->(Account)

(Customer)-[:USED]->(Device)

(Device)-[:CONNECTED_FROM]->(IPAddress)

(Customer)-[:HAS_EMAIL]->(Email)

(Customer)-[:HAS_PHONE]->(Phone)

(Customer)-[:LIVES_AT]->(Address)

(Transaction)-[:AT]->(Merchant)

(Address)-[:IN_COUNTRY]->(Country)


TSV Files
    ↓
DTOs
    ↓
Validation
    ↓
Normalization
    ↓
Graph Mapping
    ↓
Neo4j


AMLGraph
│
├── Data
│   ├── Customers.tsv
│   ├── FinancialInstitutions.tsv
│   ├── Accounts.tsv
│   ├── Transactions.tsv
│   ├── CashEvents.tsv
│   ├── Devices.tsv
│   ├── IPAddresses.tsv
│   └── ...
│
├── Domain.fs
│
├── Readers
│   ├── DelimitedReader.fs
│   ├── CustomerReader.fs
│   ├── AccountReader.fs
│   ├── TransactionReader.fs
│   └── ...
│
├── Graph
│   ├── Neo4j.fs
│   ├── CustomerNodes.fs
│   ├── AccountNodes.fs
│   ├── InstitutionNodes.fs
│   ├── TransactionNodes.fs
│   └── RelationshipBuilder.fs
│
├── Analysis
│   ├── Structuring.fs
│   ├── SharedDevice.fs
│   ├── CircularMoneyFlow.fs
│   └── ...
│
└── Program.fs