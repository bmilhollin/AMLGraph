namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Customer =

    let private toParameters (customer:Customer) =
        dict [
            "customerId", box (Unwrap.customerId customer.CustomerId)
            "institutionId", box (Unwrap.institutionId customer.InstitutionId)
            "personId", box (Unwrap.personId customer.PersonId)
            "riskRating", box customer.RiskRating
        ]

    let create (customers:Customer list) =

        let cypher =
            """
            MERGE (c:Customer   {
                customerId:$customerId, 
                institutionId:$institutionId
                })
            SET
                c.personId = $personId,
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