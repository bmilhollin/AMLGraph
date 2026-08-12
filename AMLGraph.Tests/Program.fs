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
                    Validation.Person.tests
                    Validation.Institution.tests
                    Validation.Customer.tests
                    Validation.Account.tests
                    Validation.Ownership.tests
                ]
        )