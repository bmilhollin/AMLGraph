namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Transaction =

    let private toParameters (transaction: Transaction) =

        let fromAccountId, fromInstitutionId = EntityIds.uniqueAccountIdValue transaction.FromAccount
        let toAccountId, toInstitutionId = EntityIds.uniqueAccountIdValue transaction.ToAccount

        dict [
            "transactionId", box (EntityIds.transactionIdValue transaction.TransactionId)
            "timestamp", box transaction.Timestamp
            "fromAccountId", box (EntityIds.accountIdValue fromAccountId)
            "fromInstitutionId", box (EntityIds.institutionIdValue fromInstitutionId)
            "toAccountId", box (EntityIds.accountIdValue toAccountId)
            "toInstitutionId", box (EntityIds.institutionIdValue toInstitutionId)
            "sentAmount", box transaction.Sent.Amount
            "sentCurrency", box (TryParse.currencyString transaction.Sent.Currency)
            "receivedAmount", box transaction.Received.Amount
            "receivedCurrency", box (TryParse.currencyString transaction.Received.Currency)
            "format", box (TryParse.paymentFormatString transaction.Format)
        ]
    let create (transactions: Transaction list) =

        let cypher =
            """
            MERGE (t:Transaction {transactionId:$transactionId, fromInstitutionId:$fromInstitutionId})
            SET
                t.timestamp = $timestamp,
                t.fromAccountId = $fromAccountId,
                t.toAccountId = $toAccountId,
                t.toInstitutionId = $toInstitutionId,
                t.sentAmount = $sentAmount,
                t.sentCurrency = $sentCurrency,
                t.receivedAmount = $receivedAmount,
                t.receivedCurrency = $receivedCurrency,
                t.format = $format
            """

        async {
            for transaction in transactions do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters transaction)

            printfn "Transaction nodes created"
        }