namespace AMLGraph.Validation

open AMLGraph.Domain

module Account =

    let private accountInstituteAttributesMatch left right =
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

    let private missingInstitution accountId =
        {
            Entity = AccountKey accountId
            Issue = MissingInstitution
        }

    /// An accountId and institutionId define a unique account key
    /// The pair of accountId and institutionId could exists on multiple rows within Accounts.tsv.
    /// Likewise, an account/institution pair can have multiple owners (CustomerId),
    /// but all the other fields must be identical to be considered a valid account with consistent attributes.
    /// If an account/institution pair has multiple rows and fields other than CustomerId are different, it is considered an account with conflicting attributes,
    /// and the account will not be used in the graph. Conflicted accounts are captured for review.
    let validate (validInstitutions: Set<InstitutionId>) (accounts: Account list) : Validated<Account list> =

        let validAccounts = ResizeArray<Account>()
        let errors = ResizeArray<ValidationError>()

        let isValidInstitution (account: Account) = 
            if validInstitutions.Contains(account.InstitutionId) then
                validAccounts.Add(account)
            else
                errors.Add(missingInstitution account.AccountId)

        let groups =
            accounts
            |> List.groupBy (fun a -> a.AccountId, a.InstitutionId)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        for (_, group) in singletonGroups do
            isValidInstitution group.Head           

        for (_, group) in duplicateGroups do

            match group with
            | account :: others ->
                if List.forall (accountInstituteAttributesMatch account) others then
                    isValidInstitution account
                else
                    errors.Add(conflictingAccount account.AccountId)
                    if not (validInstitutions.Contains(account.InstitutionId)) then   
                        errors.Add(missingInstitution account.AccountId)
            | [] ->
                invalidOp "Unexpected empty account/institution group."


        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }