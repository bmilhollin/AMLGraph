namespace AMLGraph

module CustomerNode =

    let create (customers:Customer list) =

        task {

            for customer in customers do

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

                use session = Neo4j.driver.AsyncSession()

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