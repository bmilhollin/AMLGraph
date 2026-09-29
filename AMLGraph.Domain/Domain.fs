namespace AMLGraph.Domain

open System

type PersonId = PersonId of string
type InstitutionId = InstitutionId of string
type CustomerId = CustomerId of string
type AccountId = AccountId of string
type TransactionId = TransactionId of string 
type UniqueCustomerId = UniqueCustomerId of (CustomerId * InstitutionId)
type UniqueAccountId = UniqueAccountId of (AccountId * InstitutionId)
type UniqueOwnershipId = UniqueOwnershipId of (UniqueCustomerId * UniqueAccountId)
type UniqueTransactionId = UniqueTransactionId of (TransactionId * InstitutionId) 

module EntityIds =    
    let personIdValue (PersonId id) = id
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id
    let transactionIdValue (TransactionId id) = id 
    let uniqueCustomerIdValue (UniqueCustomerId (customerId, institutionId)) = (customerId, institutionId)
    let uniqueAccountIdValue (UniqueAccountId (accountId, institutionId)) = (accountId, institutionId)
    let uniqueOwnershipIdValue (UniqueOwnershipId (uniqueCustomerId, uniqueAccountId)) = (uniqueCustomerId, uniqueAccountId)
    let uniqueTransactionIdValue (UniqueTransactionId (transactionId, institutionId)) = (transactionId, institutionId)

type AccountType =
    | Checking
    | Savings
    | Business
    | CreditCard
    | Loan
    | Brokerage

type Person =
    {
        PersonId: PersonId
        FirstName: string
        LastName: string
        Dob: string
        Occupation: string
    }

type Customer =
    {
        CustomerId: CustomerId
        InstitutionId: InstitutionId
        PersonId: PersonId
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

type Currency =
    | AUD
    | BRL
    | CAD
    | CHF
    | CNY
    | EUR
    | GBP
    | INR
    | JPY
    | MXN
    | RUB
    | SGD
    | TRY
    | USD
    | Bitcoin

type PaymentFormat =
    | ACH
    | Cash
    | Cheque
    | CreditCard
    | Reinvestment
    | Wire

type Funds = 
    {
        Amount: decimal
        Currency: Currency
    }
type Transaction =
    {
        TransactionId : TransactionId
        Timestamp : DateTime
        FromAccount: UniqueAccountId
        ToAccount: UniqueAccountId
        Sent: Funds
        Received: Funds
        Format: PaymentFormat
    }
    member this.Key : UniqueTransactionId =
        // instituitionId is associated with the FromAccount
        let _, institutionId = 
            EntityIds.uniqueAccountIdValue this.FromAccount
        UniqueTransactionId (this.TransactionId, institutionId)

type HasCustomerRecord =
    {
        PersonId: PersonId
        CustomerKey: UniqueCustomerId
    }
    
type HeldAt =
    {
        AccountKey: UniqueAccountId
    }

type Ownership =
    {
        CustomerKey: UniqueCustomerId
        AccountKey: UniqueAccountId
    }
    member this.Key : UniqueOwnershipId =
        UniqueOwnershipId (this.CustomerKey, this.AccountKey)

/// TransactionKey's InstitutionId is associated with the FromAccount
type Sent =
    {
        FromAccountKey : UniqueAccountId
        TransactionKey : UniqueTransactionId
    }

/// TransactionKey's InstitutionId is associated with the FromAccount
type ReceivedBy =
    {
        TransactionKey : UniqueTransactionId
        ToAccountKey : UniqueAccountId
    }

type EntityKey =
    | PersonKey of PersonId
    | CustomerKey of UniqueCustomerId
    | AccountKey of UniqueAccountId
    | InstitutionKey of InstitutionId
    | OwnershipKey of UniqueOwnershipId
    | TransactionKey of UniqueTransactionId

type ValidationIssue =
    | ConflictingPersonAttributes
    | ConflictingCustomerAttributes
    | ConflictingInstitutionAttributes
    | ConflictingAccountAttributes
    | ConflictingTransactionAttributes
    | MissingCustomer
    | MissingInstitution
    | MissingAccount
    | MissingFromInstitution
    | MissingToInstitution
    | MissingFromAccount
    | MissingToAccount
    | MismatchedInstitutions
    
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
        | "Checking" -> AccountType.Checking
        | "Savings" -> AccountType.Savings
        | "Business" -> AccountType.Business
        | "Credit Card" -> AccountType.CreditCard
        | "Loan" -> AccountType.Loan
        | "Brokerage" -> AccountType.Brokerage
        | value -> failwith $"Unknown account type '{value}'."

    let value = function
        | AccountType.Checking -> "Checking"
        | AccountType.Savings -> "Savings"
        | AccountType.Business -> "Business"
        | AccountType.CreditCard -> "Credit Card"
        | AccountType.Loan -> "Loan"
        | AccountType.Brokerage -> "Brokerage"

module Parse =

    let currency value =
        match value with
        | "AUD" | "Australian Dollar" -> AUD
        | "BRL" | "Brazil Real" -> BRL
        | "CAD" | "Canadian Dollar" -> CAD
        | "CHF" | "Swiss Franc" -> CHF
        | "CNY" | "Yuan" -> CNY
        | "EUR" | "Euro" -> EUR
        | "GBP" | "UK Pound" -> GBP
        | "INR" | "Rupee" -> INR
        | "JPY" | "Yen" -> JPY
        | "MXN" | "Mexican Peso" -> MXN
        | "RUB" | "Ruble" -> RUB
        | "SGD" | "Singapore Dollar" -> SGD
        | "TRY" | "Turkish Lira" -> TRY
        | "USD" | "US Dollar" -> USD
        | "BTC" | "Bitcoin" -> Bitcoin
        | _ -> failwith $"Unknown currency: {value}"

    let currencyString (currency: Currency) =
        match currency with
        | AUD -> "AUD"
        | BRL -> "BRL"
        | CAD -> "CAD"
        | CHF -> "CHF"
        | CNY -> "CNY"
        | EUR -> "EUR"
        | GBP -> "GBP"
        | INR -> "INR"
        | JPY -> "JPY"
        | MXN -> "MXN"
        | RUB -> "RUB"
        | SGD -> "SGD"
        | TRY -> "TRY"
        | USD -> "USD"
        | Bitcoin -> "BTC"

    let paymentFormat value =
        match value with
        | "ACH" -> PaymentFormat.ACH
        | "Cash" -> PaymentFormat.Cash
        | "Cheque" -> PaymentFormat.Cheque
        | "Credit Card" -> PaymentFormat.CreditCard
        | "Reinvestment" -> PaymentFormat.Reinvestment
        | "Wire" -> PaymentFormat.Wire
        | value -> failwith $"Unknown payment format: {value}"

    let paymentFormatString (format: PaymentFormat) =
        match format with
        | PaymentFormat.ACH -> "ACH"
        | PaymentFormat.Cash -> "Cash"
        | PaymentFormat.Cheque -> "Cheque"
        | PaymentFormat.CreditCard -> "Credit Card"
        | PaymentFormat.Reinvestment -> "Reinvestment"
        | PaymentFormat.Wire -> "Wire"
