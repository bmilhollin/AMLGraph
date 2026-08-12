namespace AMLGraph.Validation

open AMLGraph.Domain

module Ownership =

    let private missingCustomer uniqueCustomerId =
        {
            Entity = CustomerKey uniqueCustomerId
            Issue = MissingCustomer
        }

    let private missingAccount uniqueAccountId =
        {
            Entity = AccountKey uniqueAccountId
            Issue = MissingAccount
        }

    let private mismatchedInstitutions ownership=
        
        {
            Entity = (ownership.CustomerKey, ownership.AccountKey) |> OwnershipId |> OwnershipKey
            Issue = MismatchedInstitutions
        }

    let private validateOwnership validCustomerKeys validAccountKeys ownership =

        let customerExists =
            Set.contains ownership.CustomerKey validCustomerKeys

        let accountExists =
            Set.contains ownership.AccountKey validAccountKeys

        let customerInstitutionId =
            snd (EntityIds.uniqueCustomerIdValues ownership.CustomerKey)

        let accountInstitutionId =
            snd (EntityIds.uniqueAccountIdValues ownership.AccountKey)

        let errors =
            [
                if not customerExists then
                    missingCustomer ownership.CustomerKey

                if not accountExists then
                    missingAccount ownership.AccountKey

                if customerInstitutionId <> accountInstitutionId then
                    mismatchedInstitutions ownership
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
        
        let validCustomerIds =
            customers
            |> List.map (fun c -> c.Key)
            |> Set.ofList

        let validAccountKeys =
            accounts
            |> List.map (fun a -> a.Key)
            |> Set.ofList

        let validOwnerships = ResizeArray<Ownership>()
        let errors = ResizeArray<ValidationError>()

        for ownership in normalizedOwnerships do

            let validOwnership, validationErrors =
                validateOwnership validCustomerIds validAccountKeys ownership

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