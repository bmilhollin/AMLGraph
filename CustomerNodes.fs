namespace AMLGraph

module CustomerNode =

    let private toParameters (customer:Customer) =
        dict [
            "customerId", box customer.CustomerId
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

        task {

            use session = Neo4j.driver.AsyncSession()

            for customer in customers do

                do!
                    session.ExecuteWriteAsync(
                        fun tx ->
                            task {

                                let parameters =
                                    toParameters customer

                                let! _ =
                                    tx.RunAsync(
                                        cypher,
                                        parameters)

                                return ()
                            })
                    |> Async.AwaitTask

            printfn "Customers loaded"
        }