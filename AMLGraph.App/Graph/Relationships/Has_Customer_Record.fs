namespace AMLGraph.Graph.Relationships

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Has_Customer_Record =

    let private toParameters (has_customer_record:Has_Customer_Record) =

        let personId = EntityIds.personIdValue has_customer_record.PersonId

        let customerKey, institutionId = EntityIds.uniqueCustomerIdValues has_customer_record.CustomerKey
        let customerId = EntityIds.customerIdValue customerKey
        let institutionId = EntityIds.institutionIdValue institutionId

        dict [
            "personId", box personId
            "customerId", box customerId
            "institutionId", box institutionId
        ]

    let create (has_customer_records:Has_Customer_Record list) =

        let cypher =
            """
            MATCH (p:Person {personId:$personId})
            MATCH (c:Customer {customerId:$customerId, institutionId:$institutionId})
            MERGE (p)-[:HAS_CUSTOMER_RECORD]->(c)
            """

        async {

            for has_customer_record in has_customer_records do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters has_customer_record)

            printfn "Has_Customer_Record relationships created"
        }