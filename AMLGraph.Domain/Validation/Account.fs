namespace AMLGraph.Validation

open AMLGraph.Domain

module Account =

    /// An account is uniquely identified by AccountId and InstitutionId.
    /// Duplicate account records are consolidated.
    /// Accounts referencing an invalid institution are excluded and reported.
    let validate
        (validInstitutions: Set<InstitutionId>)
        (accounts: Account list) : Validated<Account list> =

        let validAccounts = ResizeArray<Account>()
        let errors = ResizeArray<ValidationError>()

        accounts
        |> List.distinctBy (fun account -> account.Key)
        |> List.iter (fun account ->
            if validInstitutions.Contains(account.InstitutionId) then
                validAccounts.Add(account)
            else
                errors.Add {
                    Entity = AccountKey account.Key
                    Issue = MissingInstitution
                }
        )

        {
            Valid = List.ofSeq validAccounts
            Errors = List.ofSeq errors
        }