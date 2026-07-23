module AMLGraph.Tests.Program

open Expecto

[<EntryPoint>]
let main argv =

    runTestsWithCLIArgs
        []
        argv
        (
            testList "AMLGraph.Tests" [
                Validation.Customer.tests
            ]
        )