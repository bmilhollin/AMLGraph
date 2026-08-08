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

    // read and validate institutions
    let institutions =
        Reader.Institution.read "Data/Institutions.tsv"
    printfn "Read %d institutions" institutions.Length

    let validatedInstitutions =
        Validation.Institution.validate institutions
    printfn "Validated %d institutions" validatedInstitutions.Valid.Length

    if not validatedInstitutions.Errors.IsEmpty then
        printfn "Found %d institution validation errors" validatedInstitutions.Errors.Length

    // read and validate accounts and ownerships
    let accounts, ownerships = 
        Reader.Account.read "Data/Accounts.tsv"
    printfn "Read %d accounts" accounts.Length
    printfn "Read %d ownerships" ownerships.Length

    let validatedAccounts =
        let validInstitutionIds =
            validatedInstitutions.Valid
            |> List.map (fun i -> i.InstitutionId)
            |> Set.ofList
        Validation.Account.validate validInstitutionIds accounts
    printfn "Validated %d accounts" validatedAccounts.Valid.Length

    if not validatedAccounts.Errors.IsEmpty then
        printfn "Found %d account validation errors" validatedAccounts.Errors.Length

    // no validation is needed for Held_At relationships since they are derived from valid accounts
    let held_ats =
        validatedAccounts.Valid
        |> List.map (fun a -> { AccountKey = a.Key })

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
    do! Graph.Nodes.Institution.create validatedInstitutions.Valid
    do! Graph.Nodes.Account.create validatedAccounts.Valid
    do! Graph.Relationships.Held_At.create held_ats
    do! Graph.Relationships.Ownership.create validatedOwnerships.Valid

    Neo4j.driver.Dispose() // move this function to infrastructure
        
}
|> Async.RunSynchronously