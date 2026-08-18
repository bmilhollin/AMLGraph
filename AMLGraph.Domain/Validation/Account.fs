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

    let private conflictingAccount uniqueAccountId =
        {
            Entity = AccountKey uniqueAccountId
            Issue = ConflictingAccountAttributes
        }

    let private missingInstitution uniqueAccountId =
        {
            Entity = AccountKey uniqueAccountId
            Issue = MissingInstitution
        }

    /// An accountId and institutionId define a unique account key
    /// The pair of accountId and institutionId could exists on multiple rows within Accounts.tsv.
    /// Likewise, an account/institution pair can have multiple owners (CustomerId),
    /// but all the other fields must be identical to be considered a valid account with consistent attributes.
    /// If an account/institution pair has multiple rows and fields other than CustomerId are different, it is considered an account with conflicting attributes,
    /// and the account will not be used in the graph. Conflicted accounts are captured for review.
    /// It is necessary to match the institutionId of the account with a valid institutionId from the Institutions.tsv file. 
    /// If an account has an institutionId that does not exist in the Institutions.tsv file, it is considered invalid because the 
    /// institutionId is significant to the uniqueCustomerId, if the institutionId is invalid, the account will not be used in the graph.
    /// Invalid accounts are captured for review.
    /// It is important NOT to perform a similar check on personId, we do not want to lose an account bc we cannot find a personId in the Persons.tsv file.
    /// The personId may be fraudulent in a money laundering context, but we still want to capture the account and its ownership for review.
    let validate 
        (validInstitutions: Set<InstitutionId>) 
        (accounts: Account list) : Validated<Account list> =

        let validAccounts = ResizeArray<Account>()
        let errors = ResizeArray<ValidationError>()

        let isValidInstitution (account: Account) = 
            if validInstitutions.Contains(account.InstitutionId) then
                validAccounts.Add(account)
            else
                errors.Add(missingInstitution (account.Key))

        let groups =
            accounts
            |> List.groupBy (fun x -> x.Key)

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
                    errors.Add(conflictingAccount account.Key)
                    if not (validInstitutions.Contains(account.InstitutionId)) then   
                        errors.Add(missingInstitution account.Key)
            | [] ->
                invalidOp "Unexpected empty account/institution group."   

        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }