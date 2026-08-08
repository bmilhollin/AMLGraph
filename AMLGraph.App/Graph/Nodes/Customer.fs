namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Customer =

    let private toParameters (customer:Customer) =
        dict [
            "customerId", box (EntityIds.customerIdValue customer.CustomerId)
            "institutionId", box (EntityIds.institutionIdValue customer.InstitutionId)
            "entityId", box (EntityIds.entityIdValue customer.EntityId)
            "riskRating", box customer.RiskRating
        ]

    let create (customers:Customer list) =

        let cypher =
            """
            MERGE (c:Customer {customerId:$customerId})
            SET
                c.institutionId = $institutionId,
                c.entityId = $entityId,
                c.riskRating = $riskRating
            """

        async {
            for customer in customers do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters customer)

            printfn "Customers nodes created"
        }