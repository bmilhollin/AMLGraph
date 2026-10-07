namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Reporting
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
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Duplicate AccountKeys with identical attributes produce one valid account"
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
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "Duplicate AccountKeys with conflicting attributes are rejected"
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

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = AccountKey SyntheticAccount.a100.Key
                                Issue = ConflictingAccountAttributes
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "Empty accounts result in no valid accounts"
                (fun () ->
                    // Arrange
                    let accounts = []

                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid accounts"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 errors"                    
                )
    

            testCase
                "A conflicting account group does not prevent unrelated valid account groups from being validated"
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

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = AccountKey SyntheticAccount.a100.Key
                                Issue = ConflictingAccountAttributes
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)

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

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = AccountKey SyntheticAccount.a100.Key
                                Issue = MissingInstitution
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Account with unknown InstitutionId is rejected and Account with valid InstitutionId is added"
                (fun () ->
                    // Arrange
                    let accounts =
                        [
                            SyntheticAccount.a400
                            SyntheticAccount.a100
                        ]
                    
                    // Act
                    let result =
                        Account.validate validatedInstitutionIds accounts

                    // Assert
                    Expect.equal
                        (result.Valid |> List.map (fun a -> a.Key))
                        [ SyntheticAccount.a100.Key ]
                        "Expected a100 to be the only valid account"

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = AccountKey SyntheticAccount.a400.Key
                                Issue = MissingInstitution
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)                   
                )

            testCase 
                "Same AccountId at different InstitutionIds are treated as separate accounts"
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

                    let accountKeys =
                        result.Valid
                        |> List.map (fun a -> a.Key)
                        |> Set.ofList

                    Expect.equal
                        accountKeys
                        (set [
                            SyntheticAccount.a100.Key
                            SyntheticAccount.a100DifferentInstitution.Key
                        ])
                        "Expected both institution-scoped account keys"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )
        ]


