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

    let private conflictingAccount accountKey =
        {
            Entity = AccountKey accountKey
            Issue = ConflictingAccountAttributes
        }

    let private missingInstitution accountKey =
        {
            Entity = AccountKey accountKey
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
                errors.Add(missingInstitution (account.Key))

        let groups =
            accounts
            |> List.groupBy (fun a -> a.Key)

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

        
        // errors
        // |> List.ofSeq
        // |> List.iter (fun x -> 
        //     let entity =
        //         match x.Entity with
        //         | CustomerKey c -> EntityIds.customerIdValue c
        //         | AccountKey a -> 
        //             let a, i = EntityIds.uniqueAccountIdValues a
        //             EntityIds.accountIdValue a + " " + EntityIds.institutionIdValue i
        //         | InstitutionKey i-> EntityIds.institutionIdValue i
            
        //     let issue = 
        //         match x.Issue with
        //         | ConflictingCustomerAttributes -> "ConflictingCustomerAttributes"
        //         | ConflictingInstitutionAttributes -> "ConflictingInstitutionAttributes"
        //         | ConflictingAccountAttributes -> "ConflictingAccountAttributes"
        //         | MissingCustomer -> "MissingCustomer"
        //         | MissingInstitution -> "MissingInstitution"
        //         | MissingAccount -> "MissingAccount"

        //     printfn $"{entity} - {issue}"    
        //     )


        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }