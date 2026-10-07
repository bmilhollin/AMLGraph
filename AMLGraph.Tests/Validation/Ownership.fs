namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Reporting
open AMLGraph.SyntheticData

module Ownership =

    [<Tests>]
    let tests =

        let validatedCustomers = SyntheticOwnership.existingCustomers
        let validatedAccounts = SyntheticOwnership.existingAccounts

        testList "Ownership Validation" [

            testCase 
                "Valid CustomerKey and valid AccountKey produce valid ownership"
                
                (fun () ->

                    // Arrange                    
                    let ownerships =
                        [
                            SyntheticOwnership.johnOwnsA100
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid ownership"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Duplicate ownership rows produce one valid ownership"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.johnOwnsA100
                            SyntheticOwnership.johnOwnsA100
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid ownership"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "An empty ownership list produces no valid ownerships and no errors"
                (fun () ->

                    // Arrange
                    let ownerships = []

                    // Act
                    let result =
                        Ownership.validate
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid ownerships"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 errors"
                )

            testCase 
                "Multiple customers may own the same account"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.johnOwnsA100
                            SyntheticOwnership.maryOwnsA100WithJohn
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        (result.Valid |> Set.ofList)
                        (set ownerships)
                        "Expected both ownership relationships"                    

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "One customer may own multiple accounts"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.johnOwnsA100
                            SyntheticOwnership.johnOwnsA200
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        2
                        "Expected 2 valid ownerships"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (result.Valid |> Set.ofList)
                        (set ownerships)
                        "Expected both ownership relationships"
                )

            testCase 
                "Valid customer referencing an unknown account is rejected"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.jamesOwnsUnknownAccount
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid ownerships"

                    Expect.equal
                        (result.Errors |> Set.ofList)
                        (set [
                                {
                                    Entity = OwnershipKey SyntheticOwnership.jamesOwnsUnknownAccount.Key
                                    Issue = MissingAccount
                                }
                                {
                                    Entity = OwnershipKey SyntheticOwnership.jamesOwnsUnknownAccount.Key
                                    Issue = MismatchedInstitutions
                                }
                            ]
                        )
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Unknown customer referencing a valid account is rejected"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.unknownCustomerOwnsA200
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid ownerships"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = OwnershipKey SyntheticOwnership.unknownCustomerOwnsA200.Key
                                Issue = MissingCustomer
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Unknown customer referencing an unknown account is rejected"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.unknownCustomerOwnsUnknownAccount
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid ownerships"

                    Expect.equal
                        (result.Errors |> Set.ofList)
                        (set [
                            {
                                Entity = OwnershipKey SyntheticOwnership.unknownCustomerOwnsUnknownAccount.Key
                                Issue = MissingCustomer
                            }
                            {
                                Entity = OwnershipKey SyntheticOwnership.unknownCustomerOwnsUnknownAccount.Key
                                Issue = MissingAccount
                            }
                            {
                                Entity = OwnershipKey SyntheticOwnership.unknownCustomerOwnsUnknownAccount.Key
                                Issue = MismatchedInstitutions
                            }
                        ])
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "An invalid ownership does not prevent unrelated valid ownerships from being validated"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.jamesOwnsUnknownAccount
                            SyntheticOwnership.johnOwnsA100
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid ownership"

                    Expect.equal
                        (result.Valid |> Set.ofList)
                        (
                            [
                                SyntheticOwnership.johnOwnsA100
                            ] 
                            |> Set.ofList
                        )
                        "Expected valid ownership relationship"

                    Expect.equal
                        (result.Errors |> Set.ofList)
                        (set [
                            {
                                Entity = OwnershipKey SyntheticOwnership.jamesOwnsUnknownAccount.Key
                                Issue = MissingAccount
                            }
                            {
                                Entity = OwnershipKey SyntheticOwnership.jamesOwnsUnknownAccount.Key
                                Issue = MismatchedInstitutions
                            }
                        ])
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Same AccountId with different InstitutionIds are treated as different accounts"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.johnOwnsA100
                            SyntheticOwnership.johnOwnsA100DifferentInstitution
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.equal
                        (result.Valid |> Set.ofList)
                        (
                            [
                                SyntheticOwnership.johnOwnsA100
                                SyntheticOwnership.johnOwnsA100DifferentInstitution
                            ] 
                            |> Set.ofList
                        )
                        "Expected both ownership relationships"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Valid customer and valid account with different InstitutionIds are rejected"
                
                (fun () ->

                    // Arrange
                    let ownerships =
                        [
                            SyntheticOwnership.mismatchedInstitutionsOwnership
                        ]
                    
                    // Act
                    let result =
                        Ownership.validate 
                            validatedCustomers
                            validatedAccounts
                            ownerships

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid ownerships"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = OwnershipKey SyntheticOwnership.mismatchedInstitutionsOwnership.Key
                                Issue = MismatchedInstitutions
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )
        ]