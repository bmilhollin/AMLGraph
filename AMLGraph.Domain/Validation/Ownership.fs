namespace AMLGraph.Validation

open AMLGraph.Domain

module Ownership =

    // Customer and Account institutions have been validated
    // in Customer and Account validations

    let private missingCustomer ownershipKey =
        {
            Entity = OwnershipKey ownershipKey
            Issue = MissingCustomer
        }

    let private missingAccount ownershipKey =
        {
            Entity =  OwnershipKey ownershipKey
            Issue = MissingAccount
        }

    let private mismatchedInstitutions ownershipKey=
        
        {
            Entity = OwnershipKey ownershipKey
            Issue = MismatchedInstitutions
        }

    let private validateOwnership 
        validCustomerKeys 
        validAccountKeys 
        ownership =

        let customerExists =
            Set.contains ownership.CustomerKey validCustomerKeys

        let accountExists =
            Set.contains ownership.AccountKey validAccountKeys

        let customerInstitutionId =
            snd (EntityIds.uniqueCustomerIdValue ownership.CustomerKey)

        let accountInstitutionId =
            snd (EntityIds.uniqueAccountIdValue ownership.AccountKey)

        let errors =
            [
                if not customerExists then
                    missingCustomer ownership.Key

                if not accountExists then
                    missingAccount ownership.Key

                if customerInstitutionId <> accountInstitutionId then
                    mismatchedInstitutions ownership.Key
            ]

        if List.isEmpty errors then
            Some ownership, []
        else
            None, errors

    let validate 
        (customers: Customer list) 
        (accounts: Account list) 
        (ownerships: Ownership list) : Validated<Ownership list> =

        let normalizedOwnerships =
            ownerships
            |> List.distinct
        
        let validCustomerKeys =
            customers
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validAccountKeys =
            accounts
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validOwnerships = ResizeArray<Ownership>()
        let errors = ResizeArray<ValidationError>()

        for ownership in normalizedOwnerships do

            let validOwnership, validationErrors =
                validateOwnership validCustomerKeys validAccountKeys ownership

            match validOwnership with
            | Some ownership ->
                validOwnerships.Add ownership
            | None ->
                ()

            errors.AddRange validationErrors

        {
            Valid = List.ofSeq validOwnerships
            Errors = List.ofSeq errors
        }