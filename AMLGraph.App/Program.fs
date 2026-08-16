module AMLGraph.Program

open AMLGraph.Domain
open AMLGraph.Reporting
open AMLGraph.Infrastructure

async {

    do! Neo4j.verifyConnectionAsync ()

    do! Schema.initializeAsync ()

    let importResult =
        Import.loadAndValidate ()

    Import.summarize importResult
    |> printfn "%s"

    ValidationReport.summarizeErrors importResult.Errors
    |> printfn "%s"

    let hasCustomerRecords =
        GraphData.hasCustomerRecords importResult.Customers.Validation.Valid

    let heldAts =
        GraphData.heldAts importResult.Accounts.Validation.Valid

    do! Graph.Nodes.Person.create importResult.Persons.Validation.Valid
    do! Graph.Nodes.Customer.create importResult.Customers.Validation.Valid
    do! Graph.Nodes.Institution.create importResult.Institutions.Validation.Valid
    do! Graph.Nodes.Account.create importResult.Accounts.Validation.Valid
    do! Graph.Nodes.Transaction.create importResult.Transactions.Validation.Valid
    do! Graph.Relationships.Has_Customer_Record.create hasCustomerRecords
    do! Graph.Relationships.Ownership.create importResult.Ownerships.Validation.Valid
    do! Graph.Relationships.Held_At.create heldAts

    Neo4j.dispose ()
        
}
|> Async.RunSynchronously