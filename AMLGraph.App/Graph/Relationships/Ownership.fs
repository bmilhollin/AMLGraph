namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Ownership =

    let private toParameters (ownership:Ownership) =

        let accountId, institutionId = EntityIds.uniqueAccountIdValues ownership.AccountKey
        let accountId = EntityIds.accountIdValue accountId
        let institutionId = EntityIds.institutionIdValue institutionId

        dict [
            "customerId", box (EntityIds.customerIdValue ownership.CustomerId)
            "accountId", box accountId
            "institutionId", box institutionId
        ]

    let create (ownerships:Ownership list) =

        let cypher =
            """
            MATCH (c:Customer {customerId:$customerId})
            MATCH (a:Account {accountId:$accountId, institutionId:$institutionId})
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