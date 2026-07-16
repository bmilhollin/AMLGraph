namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Account =

    let private toParameters (account:Account) =
        dict [
            "accountId", box account.AccountId
            "institutionId", box account.InstitutionId
            "accountType", box account.AccountType
            "openDate", box account.OpenDate
            "balance", box account.Balance
        ]

    let create (accounts:Account list) =

        let cypher =
            """
            MERGE (a:Account {accountId:$accountId})
            SET
                a.institutionId = $institutionId,
                a.accountType = $accountType,
                a.openDate = $openDate,
                a.balance = $balance
            """

        async {
            for account in accounts do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters account)

            printfn "Accounts loaded"
        }