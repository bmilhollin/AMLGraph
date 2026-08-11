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
                "A single customer with unique CustomerKey is valid"
                (fun () ->

                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                        ]
                    
                    // Act
                    let result =
                        Customer.validate customers

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
                "Duplicate CustomerKeys with identical attributes produce one valid customer" 
                (fun () ->

                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.john
                        ]

                    
                    // Act
                    let result =
                        Customer.validate customers

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
                "Duplicate CustomerKeys with conflicting attributes are rejected"
                (fun () ->
                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.johnHigherRisk
                        ]

                    // Act
                    let result =
                        Customer.validate customers
                        
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
                        (CustomerKey SyntheticCustomer.john.Key)
                        "Expected error to reference the conflicting customer"
                )

            testCase
                "Conflicting customerId groups do not prevent valid customer groups from being imported"
                (fun () ->
                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.johnHigherRisk
                            SyntheticCustomer.mary
                            SyntheticCustomer.mary
                            SyntheticCustomer.james
                        ]

                    // Act
                    let result =
                        Customer.validate customers
                        
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
                        (CustomerKey SyntheticCustomer.john.Key)
                        "Expected error to reference the conflicting customer"

                    let validIds =
                        result.Valid
                        |> List.map (fun c -> c.CustomerId)
                        |> Set.ofList

                    Expect.equal
                        validIds
                        (   
                            set [
                                    SyntheticCustomer.mary.CustomerId
                                    SyntheticCustomer.james.CustomerId
                                ]
                        )
                        "Expected Mary and James to be valid customers"
                )
        ]