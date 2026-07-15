namespace AMLGraph.Infrastructure

open Neo4j.Driver
open System.Threading.Tasks
module Neo4j =

    let uri = "bolt://localhost:7687"
    let username = "neo4j"
    let password = "FraudAlertSystem"

    let driver =
        GraphDatabase.Driver(
            uri,
            AuthTokens.Basic(username, password)
        )

    let verifyConnectionAsync () =
        async {
            do!
                driver.VerifyConnectivityAsync()
                |> Async.AwaitTask

            printfn "Verified connection"
        }

    let executeWriteAsync cypher parameters =
        async {

            use session = driver.AsyncSession()

            do!
                session.ExecuteWriteAsync(
                    fun tx ->
                        task {

                            let! _ =
                                tx.RunAsync(
                                    cypher,
                                    parameters)

                            return () //RunAsync returns an IResultCursor that we don't need to consume since we are writing
                        })
                |> Async.AwaitTask
        }