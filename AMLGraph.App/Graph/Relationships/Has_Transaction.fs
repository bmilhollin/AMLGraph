namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Has_Transaction =

    let private toParameters (posts: Has_Transaction) =

        let accountId, institutionId =
            EntityIds.uniqueAccountIdValue posts.AccountId

        let transactionId, _ =
            EntityIds.uniqueTransactionIdValue posts.TransactionId

        dict [
            "accountId", box (EntityIds.accountIdValue accountId)
            "institutionId", box (EntityIds.institutionIdValue institutionId)
            "transactionId", box (EntityIds.transactionIdValue transactionId)
        ]

    let create (posts: Has_Transaction list) =

        let cypher =
            """
           MATCH (a:Account {
                accountId: $accountId,
                institutionId: $institutionId
            })
            MATCH (t:Transaction {
                transactionId: $transactionId,
                institutionId: $institutionId
            })
            MERGE (a)-[:HAS_TRANSACTION]->(t)
            """

        async {
            for post in posts do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters post)

            printfn "HAS_TRANSACTION relationships created"
        }