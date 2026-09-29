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

    let sentTransactions, receivedTransactions =
        GraphData.sentAndReceivedBy importResult.FundsTransactions.Validation.Valid

    do! Clear.graph ()

    do! Graph.Nodes.Person.create importResult.Persons.Validation.Valid
    do! Graph.Nodes.Customer.create importResult.Customers.Validation.Valid
    do! Graph.Nodes.Institution.create importResult.Institutions.Validation.Valid
    do! Graph.Nodes.Account.create importResult.Accounts.Validation.Valid
    do! Graph.Nodes.FundsTransaction.create importResult.FundsTransactions.Validation.Valid
    do! Graph.Relationships.Has_Customer_Record.create hasCustomerRecords
    do! Graph.Relationships.Ownership.create importResult.Ownerships.Validation.Valid
    do! Graph.Relationships.Held_At.create heldAts
    do! Graph.Relationships.Sent.create sentTransactions
    do! Graph.Relationships.ReceivedBy.create receivedTransactions    

    Neo4j.dispose ()
        
}
|> Async.RunSynchronously