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

    let private validateOwnership validCustomerKeys validAccountKeys ownership =

        // Do these relationships reference existing entities?

        let customerExists =
            Set.contains ownership.CustomerKey validCustomerKeys

        let accountExists =
            Set.contains ownership.AccountKey validAccountKeys

        let customerInstitutionId = 
            snd (EntityIds.uniqueCustomerIdValues ownership.CustomerKey)
            |> EntityIds.institutionIdValue

        let accountInstitutionId = 
            snd (EntityIds.uniqueAccountIdValues ownership.AccountKey)
            |> EntityIds.institutionIdValue

        if customerInstitutionId <> accountInstitutionId then
            invalidOp ("Ownership created with single institution from accounts.tsv row, should not be able to have mismatched " + 
                "institution IDs of {customerInstitutionId} and {accountInstitutionId} for customer {customerId}")
        else
            match customerExists, accountExists with 
            | true, true ->
                Some ownership, []

            | false, true ->
                None, [ missingCustomer ownership.CustomerKey ]

            | true, false ->
                None, [ missingAccount ownership.AccountKey ]

            | false, false ->
                None,
                [
                    missingCustomer ownership.CustomerKey
                    missingAccount ownership.AccountKey
                ]

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