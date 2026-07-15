module AMLGraph.Program

open AMLGraph.Infrastructure
open AMLGraph.Readers
open AMLGraph.Graph.Nodes

async {

    do!
        Neo4j.verifyConnection()
        |> Async.AwaitTask

    do!
        Schema.initialize()

    let customers =
        DelimitedReader.readCustomersFromFile "Data/Customers.tsv"

    printfn "Read %d customers" customers.Length

    do!
        Customer.create customers

    Neo4j.driver.Dispose()
    
}
|> Async.RunSynchronously