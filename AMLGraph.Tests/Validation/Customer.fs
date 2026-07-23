namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Tests.TestData

module Customer =

    [<Tests>]
    let tests =

        testList "Customer Validation" [

            testCase 
                "A single customer with unique CustomerId is valid" 
                (fun () ->

                    // Arrange
                    let customerRows =
                        [
                            SyntheticCustomer.john
                        ]

                    
                    // Act
                    let result =
                        Customer.validate customerRows

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected one valid customer"

                    Expect.isEmpty
                        result.Errors
                        "Expected no validation errors"
                )

            testCase 
                "Duplicate customer rows with identical attributes are valid" 
                (fun () ->

                    // Arrange
                    let customerRows =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.john
                        ]

                    
                    // Act
                    let result =
                        Customer.validate customerRows

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected one valid customer"

                    Expect.isEmpty
                        result.Errors
                        "Expected no validation errors"
                )

            testCase
                "A customer with conflicting attributes is rejected"
                (fun () ->
                    // Arrange
                    let customerRows =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.johnDifferentOccupation
                        ]

                    // Act
                    let result =
                        Customer.validate customerRows
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected no valid customers"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected one validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingCustomerAttributes
                        "Expected conflicting customer attributes error"

                    Expect.equal
                        error.Entity
                        (CustomerKey SyntheticCustomer.john.CustomerId)
                        "Expected error to reference the conflicting customer"
                                    )
        ]