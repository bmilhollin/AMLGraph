namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.SyntheticData

module Customer =

    [<Tests>]
    let tests =

        testList "Customer Validation" [

            testCase 
                "A single customer row with unique CustomerId is valid"
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
                        "Expected 1 valid customer"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase 
                "Duplicate customerId rows with identical attributes are valid" 
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
                        "Expected 1 valid customer"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase
                "Duplicate customerId rows with conflicting attributes are rejected"
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
                        "Expected 0 valid customers"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

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

            testCase
                "Conflicting customerId groups do not prevent valid customer groups from being imported"
                (fun () ->
                    // Arrange
                    let customerRows =
                        [
                            SyntheticCustomer.mary
                            SyntheticCustomer.maryDifferentOccupation
                            SyntheticCustomer.james
                            SyntheticCustomer.john
                            SyntheticCustomer.john
                        ]

                    // Act
                    let result =
                        Customer.validate customerRows
                        
                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid customers"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingCustomerAttributes
                        "Expected conflicting customer attributes error"

                    Expect.equal
                        error.Entity
                        (CustomerKey SyntheticCustomer.mary.CustomerId)
                        "Expected error to reference the conflicting customer"

                    let validIds =
                        result.Valid
                        |> List.map (fun c -> c.CustomerId)
                        |> Set.ofList

                    Expect.equal
                        validIds
                        (   
                            set [
                                    SyntheticCustomer.john.CustomerId
                                    SyntheticCustomer.james.CustomerId
                                ]
                        )
                        "Expected John and James to be valid customers"
                )
        ]