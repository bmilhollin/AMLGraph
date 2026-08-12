module AMLGraph.Program

open AMLGraph.Domain
open AMLGraph.Reporting
open AMLGraph.Infrastructure

async {

    do! Neo4j.verifyConnectionAsync ()

    do! Schema.initializeAsync ()

    let importResult =
        Import.loadAndValidate ()

    ValidationReport.formatErrors importResult.Errors
    |> printfn "Validation Errors:\n%s"

    let hasCustomerRecords =
        GraphData.hasCustomerRecords importResult.Customers.Valid

    let heldAts =
        GraphData.heldAts importResult.Accounts.Valid

    do! Graph.Nodes.Person.create importResult.Persons.Valid
    do! Graph.Nodes.Customer.create importResult.Customers.Valid
    do! Graph.Nodes.Institution.create importResult.Institutions.Valid
    do! Graph.Nodes.Account.create importResult.Accounts.Valid
    do! Graph.Relationships.Has_Customer_Record.create hasCustomerRecords
    do! Graph.Relationships.Ownership.create importResult.Ownerships.Valid
    do! Graph.Relationships.Held_At.create heldAts

    Neo4j.driver.Dispose() // move this function to infrastructure
        
}
|> Async.RunSynchronously