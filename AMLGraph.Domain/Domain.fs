namespace AMLGraph.Domain

open System

type PersonId = PersonId of string
type InstitutionId = InstitutionId of string
type CustomerId = CustomerId of string
type AccountId = AccountId of string
type TransactionId = TransactionId of string
type UniqueCustomerId = UniqueCustomerId of (CustomerId * InstitutionId)
type UniqueAccountId = UniqueAccountId of (AccountId * InstitutionId)
type OwnershipId = OwnershipId of (UniqueCustomerId * UniqueAccountId)
type UniqueTransactionId = UniqueTransactionId of (TransactionId * InstitutionId)

module EntityIds =    
    let personIdValue (PersonId id) = id
    let customerIdValue (CustomerId id) = id
    let accountIdValue (AccountId id) = id
    let institutionIdValue (InstitutionId id) = id
    let transactionIdValue (TransactionId id) = id
    let uniqueCustomerIdValues (UniqueCustomerId (customerId, institutionId)) = (customerId, institutionId)
    let uniqueAccountIdValues (UniqueAccountId (accountId, institutionId)) = (accountId, institutionId)
    let uniqueOwnershipIdValues (OwnershipId (customerId, accountId)) = (customerId, accountId)
    let uniqueTransactionIdValues (UniqueTransactionId (transactionId, institutionId)) = (transactionId, institutionId)

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

type EntityKey =
    | PersonKey of PersonId
    | CustomerKey of UniqueCustomerId
    | AccountKey of UniqueAccountId
    | InstitutionKey of InstitutionId
    | OwnershipKey of OwnershipId
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
                    