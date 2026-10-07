namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Reporting
open AMLGraph.SyntheticData

module Transaction =

    [<Tests>]
    let tests =

        let validatedInstitutions = SyntheticTransaction.existingInstitutions
        let validatedAccountKeys = SyntheticTransaction.existingAccountKeys
        let original = SyntheticTransaction.t100

        let mismatchCases =
            [
                "Timestamp",
                { original with Timestamp = original.Timestamp.AddMinutes(1.0) }

                "FromAccount",
                { original with FromAccount = SyntheticAccount.a200.Key }

                "ToAccount",
                { original with ToAccount = SyntheticAccount.a500.Key }

                "Sent amount",
                { original with
                    Sent = { original.Sent with Amount = 999.00m } }

                "Sent currency",
                { original with
                    Sent = { original.Sent with Currency = EUR } }

                "Received amount",
                { original with
                    Received = { original.Received with Amount = 999.00m } }

                "Received currency",
                { original with
                    Received = { original.Received with Currency = EUR } }

                "Format",
                { original with Format = Wire }
            ]

        let mismatchTests =
            mismatchCases
            |> List.map (fun (fieldName, modified) ->
                testCase
                    $"Conflicting {fieldName} rejects transactions with the same key"
                    (fun () ->
                        let result =
                            Transaction.validate
                                validatedInstitutions
                                validatedAccountKeys
                                [original; modified]

                        Expect.isEmpty
                            result.Valid
                            "Expected the conflicting group to be rejected"

                        Expect.equal
                            result.Errors
                            [
                                {
                                    Entity = TransactionKey original.Key
                                    Issue = ConflictingTransactionAttributes
                                }
                            ]
                            "Expected one conflict error for the transaction key"
                    ))       

        testList "Transaction Validation" 
            (List.append
                mismatchTests
                [
                    testCase 
                        "With unique TransactionKeys, Valid FromAccountKey and Valid ToAccountKey produce valid transactions"
                        
                        (fun () ->

                            // Arrange                    
                            let transactions =
                                [
                                    SyntheticTransaction.t100
                                    SyntheticTransaction.t200                            
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.equal
                                result.Valid.Length
                                2
                                "Expected 2 valid transactions"

                            Expect.isEmpty
                                result.Errors
                                (ValidationReport.formatErrors result.Errors)
                        )

                    testCase 
                        "Duplicate transaction rows with identical properties produce one valid transaction"
                        
                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t100
                                    SyntheticTransaction.t100
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.equal
                                result.Valid
                                [SyntheticTransaction.t100]
                                "Expected one unchanged copy of t100"

                            Expect.isEmpty
                                result.Errors
                                (ValidationReport.formatErrors result.Errors)
                        )
                    
                    testCase 
                        "Same transaction ID at different sending institutions produces two valid transactions"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t100
                                    SyntheticTransaction.t100DifferentSendingInstitution
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.hasLength
                                result.Valid
                                2
                                "Expected 2 valid transactions"

                            Expect.isEmpty
                                result.Errors
                                (ValidationReport.formatErrors result.Errors)
                            )

                    testCase 
                        "Empty transaction list results in no valid transactions and no errors"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.isEmpty
                                result.Errors
                                "Expected 0 errors"
                            )

                    testCase 
                        "Transaction with invalid FromAccount InstitutionId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t200InvalidFromAccountInstId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                2
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingFromInstitution
                                        MissingFromAccountId
                                    ]
                                )
                                "Expected missing from institution, missing from account IDs"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t200InvalidFromAccountInstId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )

                    testCase 
                        "Transaction with invalid ToAccount InstitutionId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t200InvalidToAccountInstId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                2
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingToInstitution
                                        MissingToAccountId
                                    ]
                                )
                                "Expected missing to institution, missing to account IDs"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t200InvalidToAccountInstId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )

                    testCase 
                        "Transaction with invalid FromAccount & ToAccount InstitutionId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t200InvalidToAndFromAccountInstId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                4
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingFromInstitution
                                        MissingToInstitution
                                        MissingFromAccountId
                                        MissingToAccountId
                                    ]
                                )
                                "Expected missing to&from institution, missing to&from account IDs"

                            Expect.equal
                                result.Errors.Head.Entity
                                (TransactionKey SyntheticTransaction.t200InvalidToAndFromAccountInstId.Key)
                                "Expected error to reference the invalid UniqueTransactionId"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t200InvalidToAndFromAccountInstId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )

                    testCase 
                        "Transaction with invalid FromAccount AccountId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t300InvalidFromAccountAcctId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                1
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingFromAccountId
                                    ]
                                )
                                "Expected missing from account IDs"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t300InvalidFromAccountAcctId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )
                    
                    testCase 
                        "Transaction with invalid ToAccount AccountId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t300InvalidToAccountAcctId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                1
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingToAccountId
                                    ]
                                )
                                "Expected missing to account IDs"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t300InvalidToAccountAcctId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )

                    testCase 
                        "Transaction with invalid FromAccount & ToAccount AccountId is rejected"

                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t300InvalidToAndFromAccountAcctId
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.isEmpty
                                result.Valid
                                "Expected 0 valid transactions"

                            Expect.hasLength
                                result.Errors
                                2
                                (ValidationReport.formatErrors result.Errors)

                            Expect.equal
                                (
                                    result.Errors
                                    |> List.map (fun e -> e.Issue) 
                                    |> Set.ofList
                                )
                                (
                                set 
                                    [
                                        MissingFromAccountId
                                        MissingToAccountId
                                    ]
                                )
                                "Expected missing to&from account IDs"

                            Expect.equal
                                result.Errors.Head.Entity
                                (TransactionKey SyntheticTransaction.t300InvalidToAndFromAccountAcctId.Key)
                                "Expected error to reference the invalid UniqueTransactionId"

                            Expect.equal
                                (result.Errors
                                |> List.map (fun e -> e.Entity)
                                |> List.distinct)
                                [TransactionKey SyntheticTransaction.t300InvalidToAndFromAccountAcctId.Key]
                                "Expected error to reference the invalid UniqueTransactionId"
                            )

                    testCase 
                        "Validation failure does not prevent valid transactions from being retained"
                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t200InvalidToAccountInstId
                                    SyntheticTransaction.t100
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.equal
                                result.Valid
                                [SyntheticTransaction.t100]
                                "Expected only t100 to be retained"

                            // errors/length for t200InvalidToAccount are documented above
                        )

                    testCase 
                        "Rejecting a valid group does not affect other valid transactions"
                        (fun () ->

                            // Arrange
                            let transactions =
                                [
                                    SyntheticTransaction.t100
                                    SyntheticTransaction.t100WithDifferentFormat
                                    SyntheticTransaction.t200
                                ]
                            
                            // Act
                            let result =
                                Transaction.validate 
                                    validatedInstitutions
                                    validatedAccountKeys
                                    transactions

                            // Assert
                            Expect.equal
                                result.Valid
                                [SyntheticTransaction.t200]
                                "Expected only t200 to be retained"

                            Expect.equal
                                result.Errors
                                [
                                    {
                                        Entity = TransactionKey original.Key
                                        Issue = ConflictingTransactionAttributes
                                    }
                                ]
                                "Expected one conflict error for the transaction key"
                        )
                ]
            )