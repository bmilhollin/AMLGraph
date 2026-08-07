namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.SyntheticData

module Account =

    [<Tests>]
    let tests =

        testList "Account Validation" [

            let validatedInstitutionIds =
                        [
                            SyntheticInstitution.bank01
                            SyntheticInstitution.bank02
                            SyntheticInstitution.bank03
                        ]
                        |> List.map (fun a -> a.InstitutionId)
                        |> Set.ofList

            testCase 
                "A single account with valid InstitutionId and unique AccountId is valid"
                (fun () ->

                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                        ]
                    
                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts

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
                "Duplicate accountIds/institutionIds with identical attributes produce one valid unique account" 
                (fun () ->

                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100
                        ]

                    
                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts

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
                "Duplicate accountIds/institutionIds with conflicting attributes are rejected"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100DifferentBalance
                        ]

                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts
                        
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
                        (AccountKey SyntheticAccount.a100.Key)
                        "Expected error to reference the conflicting account"
                                    )

            testCase
                "Conflicting accountIds/institutionIds groups do not prevent valid groups from being imported"
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
                        Account.validate validatedInstitutionIds accounts
                        
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
                        (AccountKey SyntheticAccount.a100.Key)
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

            testCase 
                "Account with unknown InstitutionId is rejected"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                        ]
                    
                    // Act
                    let result =
                        Account.validate Set.empty accounts

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
                        MissingInstitution
                        "Expected missing institution error"
                )

            testCase 
                "Account with valid InstitutionId is added and account with unknown InstitutionId is rejected"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a400
                        ]
                    
                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts

                    // Assert
                    Expect.hasLength
                        result.Valid
                        1
                        "Expected 1 valid account"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        MissingInstitution
                        "Expected missing institution error"

                    Expect.equal
                        result.Valid.Head.AccountId
                        SyntheticAccount.a100.AccountId
                        "Expected valid account to be a100"

                    Expect.equal
                        result.Errors.Head.Entity
                        (AccountKey SyntheticAccount.a400.Key)
                        "Expected invalid account to be a400"
                )

            testCase 
                "Same accountId with different institutionIds are treated as separate accounts"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a100
                            SyntheticAccount.a100DifferentInstitution
                        ]
                    
                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts

                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid accounts"

                    Expect.hasLength
                        result.Errors
                        0
                        "Expected 0 validation errors"

                    let institutions = 
                        result.Valid
                        |> List.map (fun c -> c.InstitutionId)
                        |> Set.ofList

                    Expect.equal
                        institutions
                        ([ InstitutionId "SYN-FI001"; InstitutionId "SYN-FI002" ] |> Set.ofList)
                        "Expected SYN-FI001 and SYN-FI002 institutions"
                )
        ]


