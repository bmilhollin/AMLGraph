namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Sent =

    let private toParameters (sent: Sent) =

        let fromAccountId, fromInstitutionId =
            Unwrap.uniqueAccountId sent.FromAccountKey

        let transactionId, _ =
            Unwrap.uniqueTransactionId sent.TransactionKey

        dict [
            "fromAccountId", box (Unwrap.accountId fromAccountId)
            "fromInstitutionId", box (Unwrap.institutionId fromInstitutionId)
            "transactionId", box (Unwrap.transactionId transactionId)
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