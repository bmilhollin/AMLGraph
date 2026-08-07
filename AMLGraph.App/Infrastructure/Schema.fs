namespace AMLGraph.Infrastructure

module Schema =

    let private constraints =
        [
            
            """
            DROP CONSTRAINT customer_id_key IF EXISTS
            """
            
            """
            CREATE CONSTRAINT customer_id_key
            IF NOT EXISTS
            FOR (c:Customer)
            REQUIRE c.customerId IS NODE KEY
            """

            """
            DROP CONSTRAINT account_id_key IF EXISTS
            """
            
            """
            CREATE CONSTRAINT account_id_key
            IF NOT EXISTS
            FOR (a:Account)
            REQUIRE (a.accountId, a.institutionId) IS NODE KEY
            """
        ]

    let initializeAsync () =
        async {
            for statement in constraints do

                do!
                    // Constraint creation executes inside a write transaction
                    Neo4j.executeWriteAsync 
                        statement
                        (dict [])

            printfn "Initialized schema"
        }