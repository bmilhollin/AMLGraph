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

                    Expect.equal
                        (
                            result.Valid 
                            |> List.map (fun o -> o.AccountKey)
                            |> List.map EntityIds.uniqueAccountIdValues
                            |> set
                        )
                        (   
                            set [
                                    AccountId "SYN-A001", InstitutionId "SYN-FI001"
                                ]
                        )
                        "Expected single SYN-A001/SYN-FI001 unique account ID"
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
                        (
                            result.Valid 
                            |> List.map (fun o -> o.CustomerKey) 
                            |> set
                        )
                        (   
                            set [
                                    SyntheticOwnership.johnOwnsA100.CustomerKey
                                    SyntheticOwnership.johnOwnsA200.CustomerKey
                                ]
                        )
                        "Expected single SYN-C001/SYN-FI001 customer key"
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
                        result.Errors.Length
                        2
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (
                            result.Errors
                            |> List.map (fun o -> o.Issue) 
                            |> set
                        )
                        (   
                            set [
                                    ValidationIssue.MissingAccount
                                    ValidationIssue.MismatchedInstitutions
                                ]
                        )
                        "Expected MissingAccount and MismatchedInstitutions issues"
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
                        result.Errors.Length
                        1
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (
                            result.Errors
                            |> List.map (fun o -> o.Issue) 
                            |> set
                        )
                        (   
                            set [
                                    ValidationIssue.MissingCustomer
                                ]
                        )
                        "Expected MissingCustomer issues"
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
                        result.Errors.Length
                        3
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (
                            result.Errors
                            |> List.map (fun o -> o.Issue) 
                            |> set
                        )
                        (   
                            set [
                                    ValidationIssue.MissingCustomer
                                    ValidationIssue.MissingAccount
                                    ValidationIssue.MismatchedInstitutions
                                ]
                        )
                        "Expected MissingCustomer, MissingAccount, and MismatchedInstitutions issues"
                )

            testCase 
                "Invalid ownerships do not prevent valid ownerships from being imported"
                
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

                    Expect.hasLength
                        result.Errors
                        2
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (
                            result.Errors 
                            |> List.map (fun o -> o.Issue)
                            |> Set.ofList
                        )
                        (   
                            set [
                                    ValidationIssue.MissingAccount
                                    ValidationIssue.MismatchedInstitutions
                                ]
                        )
                        "Expected MissingAccount and MismatchedInstitutions issues"

                    Expect.equal
                        (result.Valid |> Set.ofList)
                        (
                            [
                                SyntheticOwnership.johnOwnsA100
                            ] 
                            |> Set.ofList
                        )
                        "Expected valid ownership relationship"
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
                        result.Valid.Length
                        2
                        "Expected 2 valid ownerships"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)

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
                        result.Errors.Length
                        1
                        (ValidationReport.formatErrors result.Errors)

                    Expect.equal
                        (result.Errors |> List.map (fun e -> e.Issue) |> Set.ofList)
                        (
                            [
                                ValidationIssue.MismatchedInstitutions
                            ] 
                            |> Set.ofList
                        )
                        "Expected single MismatchedInstitutions issue"
                )
        ]