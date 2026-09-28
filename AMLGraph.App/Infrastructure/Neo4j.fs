namespace AMLGraph.Infrastructure

open System
open System.Collections.Generic
open Neo4j.Driver
module Neo4j =

    // intital, throwaway Neo4j dev database
    let databaseName = "AMLGraph"
    let uri = "bolt://localhost:7687"
    let username = "neo4j"
    let password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD")
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

    let dispose () =
        driver.Dispose()

    let executeWriteAsync cypher parameters =
        async {

            use session = 
                driver.AsyncSession(
                    SessionConfigBuilder.ForDatabase(databaseName)
                )

            do!
                session.ExecuteWriteAsync(
                    fun tx ->
                        task {

                            let! _ =
                                tx.RunAsync(
                                    cypher,
                                    parameters)

                            // RunAsync returns an IResultCursor.
                            // Constraint creation and MERGE statements do not produce results that this application needs.
                            return ()        

                        })
                |> Async.AwaitTask
        }


module Clear =

    let emptyParameters: IDictionary<string, obj> = dict []

    let graph () =
            // Implementation for clearing the graph
            let cypher =
                """
                MATCH (n) DETACH DELETE n
                """

            async {
                
                do! 
                    Neo4j.executeWriteAsync
                        cypher
                        emptyParameters

                printfn "Graph cleared"
            }
