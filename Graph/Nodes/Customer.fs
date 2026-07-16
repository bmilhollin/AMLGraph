namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Customer =

    let private toParameters (customer:Customer) =
        dict [
            "customerId", box (EntityId.customerIdValue customer.CustomerId)
            "firstName", box customer.FirstName
            "lastName", box customer.LastName
            "dob", box customer.DOB
            "occupation", box customer.Occupation
            "riskRating", box customer.RiskRating
        ]

    let create (customers:Customer list) =

        let cypher =
            """
            MERGE (c:Customer {customerId:$customerId})
            SET
                c.firstName = $firstName,
                c.lastName = $lastName,
                c.dob = $dob,
                c.occupation = $occupation,
                c.riskRating = $riskRating
            """

        async {
            for customer in customers do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters customer)

            printfn "Customers loaded"
        }