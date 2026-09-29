namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Sent =

    let private toParameters (sent: Sent) =

        let accountId, institutionId =
            EntityIds.uniqueAccountIdValue sent.FromAccountKey

        let transactionId, transactionInstitutionId =
            EntityIds.uniqueTransactionIdValue sent.TransactionKey

        dict [
            "accountId", box (EntityIds.accountIdValue accountId)
            "accountInstitutionId", box (EntityIds.institutionIdValue institutionId)
            "transactionId", box (EntityIds.transactionIdValue transactionId)
        ]
    let create (sents: Sent list) =

        let cypher =
            """
           MATCH (a:Account {
                accountId: $accountId,
                institutionId: $institutionId
            })
            MATCH (t:FundsTransaction {
                transactionId: $transactionId,
                fromInstitutionId: $institutionId
            })
            MERGE (a)-[:SENT]->(t)
            """

        async {

            for sent in sents do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters sent)

            printfn "Sent relationships created"
        }