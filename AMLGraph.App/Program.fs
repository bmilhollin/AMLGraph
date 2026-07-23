module AMLGraph.Program

open AMLGraph.Domain
open AMLGraph.Infrastructure

async {

    do! Neo4j.verifyConnectionAsync ()

    do! Schema.initializeAsync ()

    // read and validate customers
    let customers = 
        Reader.Customer.read "Data/Customers.tsv"
    printfn "Read %d customers" customers.Length

    let validatedCustomers =
        Validation.Customer.validate customers
    printfn "Validated %d customers" validatedCustomers.Valid.Length

    if not validatedCustomers.Errors.IsEmpty then
        printfn "Found %d customer validation errors" validatedCustomers.Errors.Length

    // read and validate accounts and ownerships
    let accounts, ownerships = 
        Reader.Account.read "Data/Accounts.tsv"
        // Readers.Account.read "TestData/Accounts.tsv"
    printfn "Read %d accounts" accounts.Length
    printfn "Read %d ownerships" ownerships.Length

    let validatedAccounts =
        Validation.Account.validate accounts
    printfn "Validated %d accounts" validatedAccounts.Valid.Length

    if not validatedAccounts.Errors.IsEmpty then
        printfn "Found %d account validation errors" validatedAccounts.Errors.Length

    let validatedOwnerships =
        Validation.Ownership.validate 
            validatedCustomers.Valid
            validatedAccounts.Valid
            ownerships
    printfn "Validated %d ownerships" validatedOwnerships.Valid.Length

    if not validatedOwnerships.Errors.IsEmpty then
        printfn "Found %d ownership validation errors" validatedOwnerships.Errors.Length

    // create graph nodes and relationships
    do! Graph.Nodes.Customer.create validatedCustomers.Valid
    do! Graph.Nodes.Account.create validatedAccounts.Valid
    do! Graph.Relationships.Ownership.create validatedOwnerships.Valid

    Neo4j.driver.Dispose()
        
}
|> Async.RunSynchronously