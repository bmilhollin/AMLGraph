namespace AMLGraph.Validation

open AMLGraph.Domain

module Account =

    let private accountsMatch left right =
        left.InstitutionId = right.InstitutionId &&
        left.AccountType = right.AccountType &&
        left.OpenDate = right.OpenDate &&
        left.Balance = right.Balance

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingAccount accountId =
        {
            Entity = AccountKey accountId
            Issue = ConflictingAccountAttributes
        }

    /// An accountId could exist on multiple rows within Accounts.tsv.
    /// An account can have multiple owners (CustomerId),
    /// but all the other fields must be identical to be considered a valid account with consistent attributes.
    /// If an account has multiple rows and fields other than CustomerId are different, it is considered an account with conflicting attributes,
    /// and the account will not be used in the graph. Conflicted accounts are captured for review.
    let validate (accounts: Account list) : Validated<Account list> =

        let groups =
            accounts
            |> List.groupBy (fun a -> a.AccountId)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        let validAccounts = ResizeArray<Account>()
        let errors = ResizeArray<ValidationError>()

        for (_, group) in singletonGroups do
            validAccounts.Add(group.Head)

        for (_, group) in duplicateGroups do

            match group with
            | account :: others ->
                if List.forall (accountsMatch account) others then

                    validAccounts.Add(account)

                else

                    errors.Add(conflictingAccount account.AccountId)
            | [] ->
                invalidOp "Unexpected empty account group."

        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }