namespace AMLGraph.Validation

open AMLGraph.Domain

module Ownership =

    let private missingCustomer customerId =
        {
            Entity = CustomerKey customerId
            Issue = MissingCustomer
        }

    let private missingAccount accountId =
        {
            Entity = AccountKey accountId
            Issue = MissingAccount
        }

    let private validateOwnership validCustomerIds validAccountIds ownership =

        // Do these relationships reference existing entities?

        let customerExists =
            Set.contains ownership.CustomerId validCustomerIds

        let accountExists =
            Set.contains ownership.AccountId validAccountIds

        match customerExists, accountExists with

        | true, true ->
            Some ownership, []

        | false, true ->
            None, [ missingCustomer ownership.CustomerId ]

        | true, false ->
            None, [ missingAccount ownership.AccountId ]

        | false, false ->
            None,
            [
                missingCustomer ownership.CustomerId
                missingAccount ownership.AccountId
            ]

    let validate 
        (customers: Customer list) 
        (accounts: Account list) 
        (ownerships: Ownership list) : Validated<Ownership list> =

        // Verify that every ownership references an existing customer and account.
        
        let validCustomerIds =
            customers
            |> List.map (fun c -> c.CustomerId)
            |> Set.ofList

        let validAccountIds =
            accounts
            |> List.map (fun a -> a.AccountId)
            |> Set.ofList

        let validOwnerships = ResizeArray<Ownership>()
        let errors = ResizeArray<ValidationError>()

        for ownership in ownerships do

            let validOwnership, validationErrors =
                validateOwnership validCustomerIds validAccountIds ownership

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