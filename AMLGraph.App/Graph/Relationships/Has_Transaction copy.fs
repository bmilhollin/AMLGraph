namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Has_Transaction =

    let private toParameters (has_Transaction: Has_Transaction) =

        let accountId, institutionId =
            EntityIds.uniqueAccountIdValue has_Transaction.AccountId

        let transactionId, _ =
            EntityIds.uniqueTransactionIdValue has_Transaction.TransactionId

        dict [
            "accountId", box (EntityIds.accountIdValue accountId)
            "institutionId", box (EntityIds.institutionIdValue institutionId)
            "transactionId", box (EntityIds.transactionIdValue transactionId)
        ]

    let create (has_Transactions: Has_Transaction list) =

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
            for has_Transaction in has_Transactions do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters has_Transaction)

            printfn "HAS_TRANSACTION relationships created"
        }