module AMLGraph.Program

open AMLGraph.Infrastructure
open AMLGraph.Readers
open AMLGraph.Graph.Nodes

async {

    do! Neo4j.verifyConnectionAsync ()

    do! Schema.initialize()

    let customers =
        Readers.readCustomersFromFile "Data/Customers.tsv"

    printfn "Read %d customers" customers.Length

    do! Customer.create customers

    let accounts, ownerships =
        Readers.readAccountsFromFile "Data/Accounts.tsv"

    printfn "Read %d accounts" accounts.Length
    printfn "Read %d ownerships" ownerships.Length

    do! Account.create accounts

    Neo4j.driver.Dispose()
    
}
|> Async.RunSynchronously