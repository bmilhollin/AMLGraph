module AMLGraph.Program

async {

    do!
        Neo4j.verifyConnection()
        |> Async.AwaitTask

    do!
        Schema.initialize()

    let customerDtos =
        DelimitedReader.readCustomersFromFile "Data/Customers.tsv"

    printfn "Read %d customers" customerDtos.Length

    do!
        CustomerNode.create customerDtos

    Neo4j.driver.Dispose()

}
|> Async.RunSynchronously