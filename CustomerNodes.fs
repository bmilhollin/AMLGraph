namespace AMLGraph

module CustomerNode =

    let create (customers:Customer list) =

        task {

            use session = Neo4j.driver.AsyncSession()

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
            
            for customer in customers do

                do!
                    session.ExecuteWriteAsync(
                        fun tx ->
                            task {
                                let parameters =
                                    // Neo4j expects dictionary<string, obj> for parameters
                                    dict [
                                        "customerId", box customer.CustomerId
                                        "firstName", box customer.FirstName
                                        "lastName", box customer.LastName
                                        "dob", box customer.DOB
                                        "occupation", box customer.Occupation
                                        "riskRating", box customer.RiskRating
                                    ]

                                let! _ =
                                    tx.RunAsync(
                                        cypher,
                                        parameters)

                                return ()
                            })
                    |> Async.AwaitTask

            printfn "Customers loaded"
        }