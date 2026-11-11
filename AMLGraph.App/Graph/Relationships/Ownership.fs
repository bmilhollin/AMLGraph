namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Ownership =

    let private toParameters (ownership:Ownership) =

        let customerKey, institutionId = Unwrap.uniqueCustomerId ownership.CustomerKey
        let customerId = Unwrap.customerId customerKey
        let customerInstitutionId = Unwrap.institutionId institutionId

        let accountKey, institutionId = Unwrap.uniqueAccountId ownership.AccountKey
        let accountId = Unwrap.accountId accountKey
        let institutionId = Unwrap.institutionId institutionId

        // TODO: Check institutionId is the same for both customer and account, and throw if not.

        dict [
            "customerId", box customerId
            "accountId", box accountId
            "institutionId", box institutionId
        ]

    let create (ownerships:Ownership list) =

        let cypher =
            """
            MATCH (c:Customer {customerId:$customerId, institutionId:$institutionId})
            MATCH (a:Account {accountId:$accountId, institutionId:$institutionId})
            MERGE (c)-[:OWNERSHIP]->(a)
            """

        async {

            for ownership in ownerships do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters ownership)

            printfn "OWNERSHIP relationships created"
        }