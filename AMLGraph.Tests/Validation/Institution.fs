namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.SyntheticData

module Institution =

    [<Tests>]
    let tests =

        testList "Institution Validation" [

            testCase 
                "A single institution with unique InstitutionId is valid"
                (fun () ->

                    // Arrange
                    let institutions =
                        [
                            SyntheticInstitution.bank01
                        ]

                    
                    // Act
                    let result =
                        Institution.validate institutions

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid institution"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase 
                "Duplicate institutionIds with identical attributes produce one valid institution" 
                (fun () ->

                    // Arrange
                    let institutions =
                        [
                            SyntheticInstitution.bank01
                            SyntheticInstitution.bank01
                        ]

                    
                    // Act
                    let result =
                        Institution.validate institutions

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid institution"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase
                "Duplicate institutionIds with conflicting attributes are rejected"
                (fun () ->
                    // Arrange
                    let institutions =
                        [
                            SyntheticInstitution.bank01
                            SyntheticInstitution.bank01DifferentCountryCode
                        ]

                    // Act
                    let result =
                        Institution.validate institutions
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid institutions"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingInstitutionAttributes
                        "Expected conflicting institution attributes error"

                    Expect.equal
                        error.Entity
                        (InstitutionKey SyntheticInstitution.bank01.InstitutionId)
                        "Expected error to reference the conflicting institution"
                                    )

            testCase
                "Conflicting institutionId groups do not prevent valid institution groups from being imported"
                (fun () ->
                    // Arrange
                    let institutions =
                        [
                            SyntheticInstitution.bank01
                            SyntheticInstitution.bank01DifferentCountryCode
                            SyntheticInstitution.bank02
                            SyntheticInstitution.bank03
                        ]

                    // Act
                    let result =
                        Institution.validate institutions
                        
                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid institutions"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingInstitutionAttributes
                        "Expected conflicting institution attributes error"

                    Expect.equal
                        error.Entity
                        (InstitutionKey SyntheticInstitution.bank01.InstitutionId)
                        "Expected error to reference the conflicting institution"

                    let validIds =
                        result.Valid
                        |> List.map (fun c -> c.InstitutionId)
                        |> Set.ofList

                    Expect.equal
                        validIds
                        (   
                            set [
                                    SyntheticInstitution.bank02.InstitutionId
                                    SyntheticInstitution.bank03.InstitutionId
                                ]
                        )
                        "Expected bank02 and bank03 to be valid institutions"
                )
        ]