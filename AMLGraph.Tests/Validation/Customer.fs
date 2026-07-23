namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Tests.TestData

module Customer =

    [<Tests>]
    let tests =

        testList "Customer Validation" [

            testCase "A customer with unique CustomerId is valid" <| fun _ ->

                let result =
                    Customer.validate
                        [
                            SyntheticCustomer.john
                        ]

                Expect.equal
                    result.Valid.Length
                    1
                    "Expected one valid customer"

                Expect.isEmpty
                    result.Errors
                    "Expected no validation errors"
        ]