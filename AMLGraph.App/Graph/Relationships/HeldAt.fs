namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module HeldAt =

    let private toParameters (heldAt:HeldAt) =

        let accountKey= EntityIds.uniqueAccountIdValue heldAt.AccountKey
        let accountId = EntityIds.accountIdValue (fst accountKey)
        let institutionId = EntityIds.institutionIdValue (snd accountKey)

        dict [
            "accountId", box accountId
            "institutionId", box institutionId
        ]

    let create (heldAts: HeldAt list) =

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

            for heldAt in heldAts do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters heldAt)

            printfn "HELD_AT relationships created"
        }