namespace AMLGraph.Domain

type CustomerId = CustomerId of string
type AccountId = AccountId of string
type InstitutionId = InstitutionId of string


module EntityId =
    
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id

type Customer =
    {
        CustomerId: CustomerId
        FirstName: string
        LastName: string
        DOB: string
        Occupation: string
        RiskRating: int
    }

type FinancialInstitution =
    {
        InstitutionId: InstitutionId
        Name: string
        InstitutionType: string
        Country: string
    }

type Account =
    {
        AccountId: AccountId
        InstitutionId: InstitutionId
        AccountType: string
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
    | ConflictingAccountAttributes
    | MissingCustomer
    | MissingAccount
    
type ValidationError =
    {
        Entity: EntityKey
        Issue: ValidationIssue
    }

type AccountImportStatus =
    | Valid of Account
    | Conflicted of ValidationError

type Validated<'T> =
    {
        Valid: 'T
        Errors: ValidationError list
    }