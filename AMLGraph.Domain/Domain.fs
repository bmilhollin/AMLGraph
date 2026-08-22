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
type UniqueHas_TransactionId = UniqueHas_TransactionId of (UniqueAccountId * UniqueTransactionId) // TODO REMOVE

module EntityIds =    
    let personIdValue (PersonId id) = id
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id
    let transactionIdValue (TransactionId id) = id 
    let uniqueCustomerIdValue (UniqueCustomerId (customerId, institutionId)) = (customerId, institutionId)
    let uniqueAccountIdValue (UniqueAccountId (accountId, institutionId)) = (accountId, institutionId)
    let uniqueOwnershipIdValue (UniqueOwnershipId (customerId, accountId)) = (customerId, accountId)
    let uniqueTransactionIdValue (UniqueTransactionId (transactionId, institutionId)) = (transactionId, institutionId)
    let UniqueHas_TransactionIdValue (UniqueHas_TransactionId (accountId, institutionId)) = (accountId, institutionId) // TODO REMOVE

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

// TODO REMOVE THIS SECTION - BEGIN
// ACH is Automated Clearing House, U.S. electronic payment network banks use to move money between accounts.
// Includes Direct Deposit, Automatic bill payment, Bank-to-Bank electronic transfers, Electronic payments to businesses or people, etc.
type TransactionAction =
    | Deposit
    | Withdrawal
    | Transfer
    | Payment

type TransactionMethod =
    | Cash
    | Check
    | ACH
    | ATM
    | Wire
    | Internal
    | Card

// Action-specific method types constrain the valid transaction combinations.
// For example, a Deposit cannot be constructed with a Card method.
type DepositMethod =
    | Cash
    | Check
    | ACH

type WithdrawalMethod =
    | Cash
    | ATM

type TransferMethod =
    | ACH
    | Wire
    | Internal

type PaymentMethod =
    | Check
    | Card
    | ACH

type TransactionType =
    | Deposit of DepositMethod
    | Withdrawal of WithdrawalMethod
    | Transfer of TransferMethod
    | Payment of PaymentMethod

type Transaction =
    {
        TransactionId : TransactionId
        InstitutionId : InstitutionId
        TransactionType : TransactionType // Action/Method combination gatekeeper
        Amount : decimal
        Timestamp : DateTime
    }
    member this.Key : UniqueTransactionId =
        UniqueTransactionId (this.TransactionId, this.InstitutionId)
// TODO REMOVE THIS SECTION - END
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
type FundsTransaction =
    {
        TransactionId : TransactionId
        Timestamp : DateTime
        FromAccount: UniqueAccountId
        ToAccount: UniqueAccountId
        Paid: Funds
        Received: Funds
        Format: PaymentFormat
    }
    member this.Key : UniqueTransactionId =
        let _, institutionId = 
            EntityIds.uniqueAccountIdValue this.FromAccount
        UniqueTransactionId (this.TransactionId, institutionId)

type Has_Customer_Record =
    {
        PersonId: PersonId
        CustomerKey: UniqueCustomerId
    }
    
type Held_At =
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

// TODO REMOVE
type Has_Transaction =
    {
        AccountId : UniqueAccountId
        TransactionId : UniqueTransactionId
    }
    member this.Key : UniqueHas_TransactionId =
        UniqueHas_TransactionId (this.AccountId, this.TransactionId)

type EntityKey =
    | PersonKey of PersonId
    | CustomerKey of UniqueCustomerId
    | AccountKey of UniqueAccountId
    | InstitutionKey of InstitutionId
    | OwnershipKey of UniqueOwnershipId
    | TransactionKey of UniqueTransactionId
    | Has_TransactionKey of UniqueHas_TransactionId // TODO REMOVE

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
    | MissingTransaction  // TODO REMOVE??
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

// TODO REMOVE MODULE
module TransactionType =

    let ofString transactionType transactionMethod =
        match transactionType with
        | "Deposit" ->
            match transactionMethod with
            | "Cash" -> DepositMethod.Cash
            | "Check" -> DepositMethod.Check
            | "ACH" -> DepositMethod.ACH
            | value -> failwith $"Unknown deposit method {value}"
            |> Deposit
        | "Withdrawal" ->
            match transactionMethod with
            | "Cash" -> WithdrawalMethod.Cash
            | "ATM" -> WithdrawalMethod.ATM
            | value -> failwith $"Unknown withdrawal method {value}"
            |> Withdrawal
        | "Transfer" ->
            match transactionMethod with
            | "ACH" -> TransferMethod.ACH
            | "Wire" -> TransferMethod.Wire
            | "Internal" -> TransferMethod.Internal
            | value -> failwith $"Unknown transfer method {value}"
            |> Transfer
        | "Payment" -> 
            match transactionMethod with
            | "Check" -> PaymentMethod.Check
            | "Card" -> PaymentMethod.Card
            | "ACH" -> PaymentMethod.ACH
            | value -> failwith $"Unknown payment method {value}"
            |> Payment
        | value -> failwith $"Unknown transaction type '{value}'"

    let action transactionType =
        match transactionType with
        | Deposit _ -> TransactionAction.Deposit
        | Withdrawal _ -> TransactionAction.Withdrawal
        | Transfer _ -> TransactionAction.Transfer
        | Payment _ -> TransactionAction.Payment

    let method transactionType =
        match transactionType with
        | Deposit DepositMethod.Cash -> TransactionMethod.Cash
        | Deposit DepositMethod.Check -> TransactionMethod.Check
        | Deposit DepositMethod.ACH -> TransactionMethod.ACH

        | Withdrawal WithdrawalMethod.Cash -> TransactionMethod.Cash
        | Withdrawal WithdrawalMethod.ATM -> TransactionMethod.ATM

        | Transfer TransferMethod.ACH -> TransactionMethod.ACH
        | Transfer TransferMethod.Wire -> TransactionMethod.Wire
        | Transfer TransferMethod.Internal -> TransactionMethod.Internal

        | Payment PaymentMethod.Check -> TransactionMethod.Check
        | Payment PaymentMethod.Card -> TransactionMethod.Card
        | Payment PaymentMethod.ACH -> TransactionMethod.ACH

    let value transactionType =
        string (action transactionType),
        string (method transactionType)
                    
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

    let paymentFormat value =
        match value with
        | "ACH" -> PaymentFormat.ACH
        | "Cash" -> PaymentFormat.Cash
        | "Cheque" -> PaymentFormat.Cheque
        | "Credit Card" -> PaymentFormat.CreditCard
        | "Reinvestment" -> PaymentFormat.Reinvestment
        | "Wire" -> PaymentFormat.Wire
        | value -> failwith $"Unknown payment format: {value}"
