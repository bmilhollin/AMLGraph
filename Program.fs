module AMLGraph.Program

Neo4j.verifyConnection()
|> Async.AwaitTask
|> Async.RunSynchronously

Schema.initialize()
|> Async.AwaitTask
|> Async.RunSynchronously

let customerDtos =
    DelimitedReader.readCustomersFromFile "Data/Customers.tsv"

printfn "Read %d customers" customerDtos.Length

let customerNodes =
    CustomerNode.create customerDtos

customerNodes
|> Async.AwaitTask
|> Async.RunSynchronously

Neo4j.driver.Dispose()