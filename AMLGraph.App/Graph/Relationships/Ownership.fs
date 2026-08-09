namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Ownership =

    let private toParameters (ownership:Ownership) =

        let customerKey, institutionId = EntityIds.uniqueCustomerIdValues ownership.CustomerKey
        let customerId = EntityIds.customerIdValue customerKey
        let customerInstitutionId = EntityIds.institutionIdValue institutionId

        let accountKey, institutionId = EntityIds.uniqueAccountIdValues ownership.AccountKey
        let accountId = EntityIds.accountIdValue accountKey
        let institutionId = EntityIds.institutionIdValue institutionId

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
            MERGE (c)-[:OWNS]->(a)
            """

        async {

            for ownership in ownerships do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters ownership)

            printfn "Ownership relationships created"
        }