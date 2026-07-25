namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.SyntheticData

module Account =

    [<Tests>]
    let tests =

        testList "Account Validation" [

            testCase 
                "A single account with unique AccountId is valid"
                (fun () ->

                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                        ]

                    
                    // Act
                    let result =
                        Account.validate accounts

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid account"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase 
                "Duplicate accountIds with identical attributes produce one valid account" 
                (fun () ->

                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100
                        ]

                    
                    // Act
                    let result =
                        Account.validate accounts

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid account"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase
                "Duplicate accountIds with conflicting attributes are rejected"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100DifferentBalance
                        ]

                    // Act
                    let result =
                        Account.validate accounts
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid accounts"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingAccountAttributes
                        "Expected conflicting account attributes error"

                    Expect.equal
                        error.Entity
                        (AccountKey SyntheticAccount.a100.AccountId)
                        "Expected error to reference the conflicting account"
                                    )

            testCase
                "Conflicting account groups do not prevent valid account groups from being imported"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100DifferentBalance
                            SyntheticAccount.a200
                            SyntheticAccount.a300
                            SyntheticAccount.a300
                        ]

                    // Act
                    let result =
                        Account.validate accounts
                        
                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid accounts"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingAccountAttributes
                        "Expected conflicting account attributes error"

                    Expect.equal
                        error.Entity
                        (AccountKey SyntheticAccount.a100.AccountId)
                        "Expected error to reference the conflicting account"

                    let validIds =
                        result.Valid
                        |> List.map (fun c -> c.AccountId)
                        |> Set.ofList

                    Expect.equal
                        validIds
                        (   
                            set [
                                    SyntheticAccount.a200.AccountId
                                    SyntheticAccount.a300.AccountId
                                ]
                        )
                        "Expected a200 and a300 to be valid accounts"
                )
        ]