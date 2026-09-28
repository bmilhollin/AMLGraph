namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module FundsTransaction =

    let private toParameters (fundsTransaction: FundsTransaction) =

        let fromAccountId, fromInstitutionId = EntityIds.uniqueAccountIdValue fundsTransaction.FromAccount
        let toAccountId, toInstitutionId = EntityIds.uniqueAccountIdValue fundsTransaction.ToAccount

        dict [
            "transactionId", box (EntityIds.transactionIdValue fundsTransaction.TransactionId)
            "timestamp", box fundsTransaction.Timestamp
            "fromAccountId", box (EntityIds.accountIdValue fromAccountId)
            "fromInstitutionId", box (EntityIds.institutionIdValue fromInstitutionId)
            "toAccountId", box (EntityIds.accountIdValue toAccountId)
            "toInstitutionId", box (EntityIds.institutionIdValue toInstitutionId)
            "sentAmount", box fundsTransaction.Sent.Amount
            "sentCurrency", box (Parse.currencyString fundsTransaction.Sent.Currency)
            "receivedAmount", box fundsTransaction.Received.Amount
            "receivedCurrency", box (Parse.currencyString fundsTransaction.Received.Currency)
            "format", box (Parse.paymentFormatString fundsTransaction.Format)
        ]
    let create (fundsTransactions:FundsTransaction list) =

        let cypher =
            """
            MERGE (t:FundsTransaction {transactionId:$transactionId, fromInstitutionId:$fromInstitutionId})
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
            for fundsTransaction in fundsTransactions do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters fundsTransaction)

            printfn "FundsTransaction nodes created"
        }