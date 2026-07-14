namespace AMLGraph

module Schema =

    let initialize() =

        task {

            use session = Neo4j.driver.AsyncSession()

            let cypher =
                """
                CREATE CONSTRAINT customer_id_key
                IF NOT EXISTS
                FOR (c:Customer)
                REQUIRE c.customerId IS NODE KEY
                """

            do!
                session.ExecuteWriteAsync(
                    fun tx ->
                        task {
                            let! _ = tx.RunAsync(cypher)
                            return ()
                        })

            printfn "Initialized schema"
        }