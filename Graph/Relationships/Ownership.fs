namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Ownership =

    let private toParameters (ownership:Ownership) =
        dict [
            "customerId", box (EntityId.customerIdValue ownership.CustomerId)
            "accountId", box (EntityId.accountIdValue ownership.AccountId)
        ]

    let create (ownerships:Ownership list) =

        let cypher =
            """
            MATCH (c:Customer {customerId:$customerId})
            MATCH (a:Account {accountId:$accountId})
            MERGE (c)-[:OWNS]->(a)
            """

        async {

            for ownership in ownerships do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters ownership)

            printfn "Ownership relationships created"
        }