namespace AMLGraph.Domain

type CustomerId = CustomerId of string
type AccountId = AccountId of string
type InstitutionId = InstitutionId of string
type AccountKey = AccountKey of AccountId * InstitutionId


module EntityIds =
    
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id

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
        FirstName: string
        LastName: string
        DOB: string
        Occupation: string
        RiskRating: int
    }

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

type Ownership =
    {
        CustomerId: CustomerId
        AccountId: AccountId
    }

type EntityKey =
    | CustomerKey of CustomerId
    | AccountKey of AccountId
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