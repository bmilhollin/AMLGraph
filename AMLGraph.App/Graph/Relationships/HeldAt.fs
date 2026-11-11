namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module HeldAt =

    let private toParameters (heldAt:HeldAt) =

        let accountKey= Unwrap.uniqueAccountId heldAt.AccountKey
        let accountId = Unwrap.accountId (fst accountKey)
        let institutionId = Unwrap.institutionId (snd accountKey)

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