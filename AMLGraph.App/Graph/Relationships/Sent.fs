namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Sent =

    let private toParameters (sent: Sent) =

        let fromAccountId, fromInstitutionId =
            EntityIds.uniqueAccountIdValue sent.FromAccountKey

        let transactionId, _ =
            EntityIds.uniqueTransactionIdValue sent.TransactionKey

        dict [
            "fromAccountId", box (EntityIds.accountIdValue fromAccountId)
            "fromInstitutionId", box (EntityIds.institutionIdValue fromInstitutionId)
            "transactionId", box (EntityIds.transactionIdValue transactionId)
        ]
    let create (sents: Sent list) =

        let cypher =
            """
           MATCH (a:Account {
                accountId: $fromAccountId,
                institutionId: $fromInstitutionId
            })
            MATCH (t:Transaction {
                transactionId: $transactionId,
                fromInstitutionId: $fromInstitutionId
            })
            MERGE (a)-[:SENT]->(t)
            """

        async {

            for sent in sents do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters sent)

            printfn "SENT relationships created"
        }