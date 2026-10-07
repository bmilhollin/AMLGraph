namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Reporting
open AMLGraph.SyntheticData

module Customer =

    [<Tests>]
    let tests =

        testList "Customer Validation" [

            let validatedInstitutionIds =
                [
                    SyntheticInstitution.bank01
                    SyntheticInstitution.bank02
                    SyntheticInstitution.bank03
                ]
                |> List.map (fun a -> a.InstitutionId)
                |> Set.ofList

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
                        Customer.validate validatedInstitutionIds customers

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid customer"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
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
                        Customer.validate validatedInstitutionIds customers

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid customer"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
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
                        Customer.validate validatedInstitutionIds customers
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid customers"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Issue = ConflictingCustomerAttributes
                                Entity = CustomerKey SyntheticCustomer.john.Key
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "Empty customer list produces no valid customers and no errors"
                (fun () ->
                    // Arrange
                    let customers = []

                    // Act
                    let result =
                        Customer.validate validatedInstitutionIds customers
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid customers"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 errors"
                )


            testCase
                "A conflicting customer group does not prevent unrelated valid customer groups from being validated"
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
                        Customer.validate validatedInstitutionIds customers
                        
                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid customers"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Issue = ConflictingCustomerAttributes
                                Entity = CustomerKey SyntheticCustomer.john.Key
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)

                    let validKeys =
                        result.Valid
                        |> List.map (fun c -> c.Key)
                        |> Set.ofList

                    Expect.equal
                        validKeys
                        (   
                            set [
                                SyntheticCustomer.mary.Key
                                SyntheticCustomer.james.Key
                            ]
                        )
                        "Expected Mary and James to be valid customers"
                )

            testCase 
                "Customer with unknown InstitutionId is rejected"
                (fun () ->
                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                        ]
                    
                    // Act
                    let result =
                        Customer.validate Set.empty customers

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid customers"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Issue = MissingInstitution
                                Entity = CustomerKey SyntheticCustomer.john.Key
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Customer with unknown InstitutionId is rejected and Customer with valid InstitutionId is added"
                (fun () ->
                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.jamesWithInvalidInstitutionId
                            SyntheticCustomer.john
                        ]
                    
                    // Act
                    let result =
                        Customer.validate validatedInstitutionIds customers

                    // Assert
                    Expect.equal
                        (result.Valid |> List.map (fun c -> c.Key))
                        [ SyntheticCustomer.john.Key ]
                        "Expected John to be the only valid customer"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Issue = MissingInstitution
                                Entity = CustomerKey SyntheticCustomer.jamesWithInvalidInstitutionId.Key
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)                    
                )

            testCase
                "Same CustomerId at different InstitutionIds are treated as different customers"
                (fun () ->

                    // Arrange
                    let customers =
                        [
                            SyntheticCustomer.john
                            SyntheticCustomer.johnDifferentInstitution
                        ]

                    // Act
                    let result =
                        Customer.validate validatedInstitutionIds customers

                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid customers"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)

                    let validKeys =
                        result.Valid
                        |> List.map (fun c -> c.Key)
                        |> Set.ofList

                    Expect.equal
                        validKeys
                        (
                            set [
                                SyntheticCustomer.john.Key
                                SyntheticCustomer.johnDifferentInstitution.Key
                            ]
                        )
                        "Expected same CustomerId at different institutions to produce distinct customer keys"
                )           
        ]