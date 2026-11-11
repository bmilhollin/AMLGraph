namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module HasCustomerRecord =

    let private toParameters (hasCustomerRecord:HasCustomerRecord) =

        let personId = Unwrap.personId hasCustomerRecord.PersonId

        let customerKey, institutionId = Unwrap.uniqueCustomerId hasCustomerRecord.CustomerKey
        let customerId = Unwrap.customerId customerKey
        let institutionId = Unwrap.institutionId institutionId

        dict [
            "personId", box personId
            "customerId", box customerId
            "institutionId", box institutionId
        ]

    let create (hasCustomerRecords:HasCustomerRecord list) =

        let cypher =
            """
            MATCH (p:Person {personId:$personId})
            MATCH (c:Customer {customerId:$customerId, institutionId:$institutionId})
            MERGE (p)-[:HAS_CUSTOMER_RECORD]->(c)
            """

        async {

            for hasCustomerRecord in hasCustomerRecords do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters hasCustomerRecord)

            printfn "HAS_CUSTOMER_RECORD relationships created"
        }