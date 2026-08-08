namespace AMLGraph.Domain

type EntityId = EntityId of string
type CustomerId = CustomerId of string
type AccountId = AccountId of string
type InstitutionId = InstitutionId of string
type UniqueCustomerId = UniqueCustomerId of (CustomerId * InstitutionId)
type UniqueAccountId = UniqueAccountId of (AccountId * InstitutionId)

module EntityIds =    
    let entityIdValue (EntityId id) = id
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id
    let uniqueCustomerIdValues (UniqueCustomerId (customerId, institutionId)) = (customerId, institutionId)
    let uniqueAccountIdValues (UniqueAccountId (accountId, institutionId)) = (accountId, institutionId)

type AccountType =
    | Checking
    | Savings
    | Business
    | CreditCard
    | Loan
    | Brokerage

type Customer =
    {
        CustomerId: CustomerId
        InstitutionId: InstitutionId
        EntityId: EntityId
        RiskRating: int
    }
    member this.Key =
        UniqueCustomerId (this.CustomerId, this.InstitutionId)


type Institution =
    {
        InstitutionId: InstitutionId
        Name: string
        InstitutionType: string
        CountryCode: string  // ISO alpha-2: US, GB, DE, etc.
    }

type Account =
    {
        AccountId: AccountId
        InstitutionId: InstitutionId
        AccountType: AccountType
        OpenDate: string
        Balance: decimal
    }
    member this.Key =
        UniqueAccountId (this.AccountId, this.InstitutionId)

type Held_At =
    {
        AccountKey: UniqueAccountId
    }

type Ownership =
    {
        CustomerKey: UniqueCustomerId
        AccountKey: UniqueAccountId
    }

type EntityKey =
    | CustomerKey of UniqueCustomerId
    | AccountKey of UniqueAccountId
    | InstitutionKey of InstitutionId

type ValidationIssue =
    | ConflictingCustomerAttributes
    | ConflictingInstitutionAttributes
    | ConflictingAccountAttributes
    | MissingCustomer
    | MissingInstitution
    | MissingAccount
    
type ValidationError =
    {
        Entity: EntityKey
        Issue: ValidationIssue
    }

type Validated<'T> =
    {
        Valid: 'T
        Errors: ValidationError list
    }

module AccountType =

    let ofString = function
        | "Checking" -> Checking
        | "Savings" -> Savings
        | "Business" -> Business
        | "Credit Card" -> CreditCard
        | "Loan" -> Loan
        | "Brokerage" -> Brokerage
        | value -> failwith $"Unknown account type '{value}'."

    let value = function
        | Checking -> "Checking"
        | Savings -> "Savings"
        | Business -> "Business"
        | CreditCard -> "Credit Card"
        | Loan -> "Loan"
        | Brokerage -> "Brokerage"