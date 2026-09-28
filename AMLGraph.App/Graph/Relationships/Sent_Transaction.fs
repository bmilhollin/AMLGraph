namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Sent_Transaction =

    let private toParameters (sent_transaction:Sent_Transaction) =
        // {
        //     FromAccountKey : UniqueAccountId
        //     TransactionKey : UniqueTransactionId
        // }
        // let uniqueAccountIdValue (UniqueAccountId (accountId, institutionId)) = (accountId, institutionId)

        let accountId, institutionId = EntityIds.uniqueAccountIdValue sent_transaction.FromAccountKey

        // type UniqueTransactionId = UniqueTransactionId of (TransactionId * InstitutionId) 
        let transactionId, _ = EntityIds.uniqueTransactionIdValue sent_transaction.TransactionKey


        dict [
            "accountId", box accountId
            "institutionId", box institutionId
            "transactionId", box transactionId
        ]

    let create (sent_transactions:Sent_Transaction list) =

        let cypher =
            """
            MATCH (p:Account {accountId:$accountId, institutionId:$institutionId})
            MATCH (c:FundsTransaction {transactionId:$transactionId, institutionId:$institutionId})
            MERGE (p)-[:SENT_TRANSACTION]->(c)
            """

        async {

            for has_customer_record in has_customer_records do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters has_customer_record)

            printfn "Has_Customer_Record relationships created"
        }