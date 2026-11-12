namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Account =

    let private toParameters (account:Account) =
        dict [
            "accountId", box (Unwrap.accountId account.AccountId)
            "institutionId", box (Unwrap.institutionId account.InstitutionId)
        ]

    let create (accounts:Account list) =

        let cypher =
            """
            MERGE (a:Account {accountId:$accountId, institutionId:$institutionId})
            """

        async {
            for account in accounts do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters account)

            printfn "Account nodes created"
        }