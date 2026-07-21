namespace AMLGraph.Validation

open AMLGraph.Domain

module Account =

    let private accountsMatch left right =
        left.InstitutionId = right.InstitutionId &&
        left.AccountType = right.AccountType &&
        left.OpenDate = right.OpenDate &&
        left.Balance = right.Balance

    let private conflictingAccount accountId =
        {
            Entity = AccountKey accountId
            Issue = ConflictingAccountAttributes
        }

    /// An accountId can exists on multiple rows within Accounts.tsv.
    /// An account can have multiple owners (CustomerId),
    /// but all the other fields must be identical to be valid.
    let validate (accounts: Account list) : Validated<Account list> =

        let groups =
            accounts
            |> List.groupBy (fun a -> a.AccountId)

        let validAccounts = ResizeArray<Account>()
        let errors = ResizeArray<ValidationError>()

        for (_, group) in groups do

            match group with

            | [] ->
                () // shouldn't happen, included for completeness

            | [account] ->
                validAccounts.Add(account)

            | account :: others ->

                if List.forall (accountsMatch account) others then

                    validAccounts.Add(account)

                else

                    errors.Add(conflictingAccount account.AccountId)

        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }