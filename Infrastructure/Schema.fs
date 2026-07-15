namespace AMLGraph.Infrastructure

module Schema =

    let initialize() =

        let cypher =
            """
            CREATE CONSTRAINT customer_id_key
            IF NOT EXISTS
            FOR (c:Customer)
            REQUIRE c.customerId IS NODE KEY
            """
        async {

            use session = Neo4j.driver.AsyncSession()   

            do!
                session.ExecuteWriteAsync(
                    fun tx ->
                        task {
                            let! _ = tx.RunAsync(cypher)
                            return ()
                        })
                |> Async.AwaitTask

            printfn "Initialized schema"
        }