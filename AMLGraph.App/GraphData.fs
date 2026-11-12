namespace AMLGraph

open AMLGraph.Domain

module GraphData =

    // No validation is needed for HELD_AT relationships
    // because they are derived from validated accounts.
    let heldAts (accounts: Account list) =
        accounts
        |> List.map (fun a ->
            {
                AccountKey = a.Key
            })

    // Derive both relationships from validated transactions;
    // their account endpoints were checked during transaction validation.
    let sentAndReceivedBy (transactions: Transaction list) =
        transactions
        |> List.map (fun t ->
            {
                FromAccountKey = t.FromAccount
                TransactionKey = t.Key
            },
            {
                TransactionKey = t.Key
                ToAccountKey = t.ToAccount
            })
        |> List.unzip