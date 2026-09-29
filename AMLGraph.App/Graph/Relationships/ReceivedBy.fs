namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module ReceivedBy =

    let private toParameters (receivedBy: ReceivedBy) =

        let transactionId, fromInstitutionId =
            EntityIds.uniqueTransactionIdValue receivedBy.TransactionKey

        let toAccountId, toInstitutionId =
            EntityIds.uniqueAccountIdValue receivedBy.ToAccountKey

        dict [
            "toAccountId", box (EntityIds.accountIdValue toAccountId)
            "toInstitutionId", box (EntityIds.institutionIdValue toInstitutionId)
            "transactionId", box (EntityIds.transactionIdValue transactionId)
            "fromInstitutionId", box (EntityIds.institutionIdValue fromInstitutionId)
        ]
    let create (receivedBys: ReceivedBy list) =

        let cypher =
            """
            MATCH (t:Transaction {
                transactionId: $transactionId,
                fromInstitutionId: $fromInstitutionId
            })
           MATCH (a:Account {
                accountId: $toAccountId,
                institutionId: $toInstitutionId
            })
            MERGE (t)-[:RECEIVED_BY]->(a)
            """

        async {

            for receivedBy in receivedBys do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters receivedBy)

            printfn "RECEIVED_BY relationships created"
        }