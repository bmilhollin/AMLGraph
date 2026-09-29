namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Held_At =

    let private toParameters (held_at:Held_At) =

        let accountKey= EntityIds.uniqueAccountIdValue held_at.AccountKey
        let accountId = EntityIds.accountIdValue (fst accountKey)
        let institutionId = EntityIds.institutionIdValue (snd accountKey)

        dict [
            "accountId", box accountId
            "institutionId", box institutionId
        ]

    let create (held_ats: Held_At list) =

        let cypher =
            """
            MATCH (a:Account {
                accountId: $accountId,
                institutionId: $institutionId
            })
            MATCH (i:Institution {
                institutionId: $institutionId
            })
            MERGE (a)-[:HELD_AT]->(i)
            """

        async {

            for held_at in held_ats do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters held_at)

            printfn "Held_At relationships created"
        }