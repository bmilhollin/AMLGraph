namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Transaction =

    let private toParameters (transaction: Transaction) =

        let fromAccountId, fromInstitutionId = Unwrap.uniqueAccountId transaction.FromAccount
        let toAccountId, toInstitutionId = Unwrap.uniqueAccountId transaction.ToAccount

        dict [
            "transactionId", box (Unwrap.transactionId transaction.TransactionId)
            "timestamp", box transaction.Timestamp
            "fromAccountId", box (Unwrap.accountId fromAccountId)
            "fromInstitutionId", box (Unwrap.institutionId fromInstitutionId)
            "toAccountId", box (Unwrap.accountId toAccountId)
            "toInstitutionId", box (Unwrap.institutionId toInstitutionId)
            "sentAmount", box transaction.Sent.Amount
            "sentCurrency", box (Parse.currencyString transaction.Sent.Currency)
            "receivedAmount", box transaction.Received.Amount
            "receivedCurrency", box (Parse.currencyString transaction.Received.Currency)
            "format", box (Parse.paymentFormatString transaction.Format)
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