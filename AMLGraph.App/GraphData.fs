namespace AMLGraph

open AMLGraph.Domain

module GraphData =


    // A validated customer may reference a Person that does not exist.
    // In AMLGraph this is intentional because unresolved identity can itself
    // represent meaningful AML information.
    let hasCustomerRecords (customers: Customer list) =
        customers
        |> List.map (fun c ->
            {
                PersonId = c.PersonId
                CustomerKey = c.Key
            })

     // no validation is needed for Held_At relationships since they are derived from valid accounts
    let heldAts (accounts: Account list) =
        accounts
        |> List.map (fun a ->
            {
                AccountKey = a.Key
            })