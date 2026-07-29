module AMLGraph.Tests.Program

open Expecto

[<EntryPoint>]
let main argv =

    runTestsWithCLIArgs
        []
        argv
        (
            testList "AMLGraph.Tests" 
                [
                    Validation.Customer.tests
                    Validation.Institution.tests
                    Validation.Account.tests
                    Validation.Ownership.tests
                ]
        )