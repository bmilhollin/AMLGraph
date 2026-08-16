namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Transaction =

    let private toParameters (transaction:Transaction) =
        dict [
            "transactionId", box (EntityIds.transactionIdValue transaction.TransactionId)
            "institutionId", box (EntityIds.institutionIdValue transaction.InstitutionId)
            "transactionType", box (TransactionType.value transaction.TransactionType)
            "amount", box transaction.Amount
            "timestamp", box transaction.Timestamp
        ]

    let create (transactions:Transaction list) =

        let cypher =
            """
            MERGE (t:Transaction {transactionId:$transactionId, institutionId:$institutionId})
            SET
                t.transactionType = $transactionType,
                t.amount = $amount,
                t.timestamp = $timestamp
            """

        async {
            for transaction in transactions do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters transaction)

            printfn "Transaction nodes created"
        }