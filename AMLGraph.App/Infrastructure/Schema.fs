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
            REQUIRE (c.customerId, c.institutionId) IS NODE KEY
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

            """
            DROP CONSTRAINT institution_id_key IF EXISTS
            """
            
            """
            CREATE CONSTRAINT institution_id_key
            IF NOT EXISTS
            FOR (i:Institution)
            REQUIRE (i.institutionId) IS NODE KEY
            """

            """
            DROP CONSTRAINT person_id_key IF EXISTS
            """
            
            """
            CREATE CONSTRAINT person_id_key
            IF NOT EXISTS
            FOR (c:Person)
            REQUIRE (c.personId) IS NODE KEY
            """

            """
            DROP CONSTRAINT transaction_id_key IF EXISTS
            """
            
            """
            CREATE CONSTRAINT transaction_id_key
            IF NOT EXISTS
            FOR (t:Transaction)
            REQUIRE (t.transactionId, t.institutionId) IS NODE KEY
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