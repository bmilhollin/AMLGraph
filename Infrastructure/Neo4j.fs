namespace AMLGraph.Infrastructure

open Neo4j.Driver

module Neo4j =

    let uri = "bolt://localhost:7687"
    let username = "neo4j"
    let password = "FraudAlertSystem"

    let driver =
        GraphDatabase.Driver(
            uri,
            AuthTokens.Basic(username, password)
        )

    let verifyConnection () =
        task {
            do!
                driver.VerifyConnectivityAsync()

            printfn "Verified connection"
        }