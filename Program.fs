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

    Neo4j.driver.Dispose()
    
}
|> Async.RunSynchronously