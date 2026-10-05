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

        testList "Transaction Validation" [

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
                        result.Valid.Length
                        1
                        "Expected 1 valid transaction"

                    Expect.isEmpty
                        result.Errors
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Duplicate transactions with different sending institutions are 2 separate valid transactions"

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
                "Duplicate transaction with conflicting attributes are rejected"

                (fun () ->

                    // Arrange
                    let transactions =
                        [
                            SyntheticTransaction.t100
                            SyntheticTransaction.t100WithDifferentFormat
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

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingTransactionAttributes
                        "Expected conflicting transaction attributes"

                    Expect.equal
                        error.Entity
                        (TransactionKey SyntheticTransaction.t100.Key)
                        "Expected error to reference the conflicting UniqueTransactionId"
                    )

        ]