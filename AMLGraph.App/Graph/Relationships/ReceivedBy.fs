namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module ReceivedBy =

    let private toParameters (receivedBy: ReceivedBy) =

        let transactionId, fromInstitutionId =
            Unwrap.uniqueTransactionId receivedBy.TransactionKey

        let toAccountId, toInstitutionId =
            Unwrap.uniqueAccountId receivedBy.ToAccountKey

        dict [
            "toAccountId", box (Unwrap.accountId toAccountId)
            "toInstitutionId", box (Unwrap.institutionId toInstitutionId)
            "transactionId", box (Unwrap.transactionId transactionId)
            "fromInstitutionId", box (Unwrap.institutionId fromInstitutionId)
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