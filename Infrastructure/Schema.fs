namespace AMLGraph.Infrastructure

module Schema =

    let private schemaStatements =
        [
            """
            CREATE CONSTRAINT customer_id_key
            IF NOT EXISTS
            FOR (c:Customer)
            REQUIRE c.customerId IS NODE KEY
            """

            """
            CREATE CONSTRAINT account_id_key
            IF NOT EXISTS
            FOR (a:Account)
            REQUIRE a.accountId IS NODE KEY
            """
        ]

    let initialize () =
        async {
            for statement in schemaStatements do

                do!
                    Neo4j.executeWriteAsync // not officially writing, but this executes constraints
                        statement
                        (dict [])

            printfn "Initialized schema"
        }