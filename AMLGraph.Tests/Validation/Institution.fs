namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.Reporting
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
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase 
                "Duplicate InstitutionIds with identical attributes produce one valid institution" 
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
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "Duplicate InstitutionIds with conflicting attributes are rejected"
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

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = InstitutionKey SyntheticInstitution.bank01.InstitutionId
                                Issue = ConflictingInstitutionAttributes
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)
                )

            testCase
                "An empty institution list produces no valid institutions and no errors"
                (fun () ->

                    // Arrange
                    let institutions = []

                    // Act
                    let result =
                        Institution.validate institutions

                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid institutions"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 errors"
                )    

            testCase
                "A conflicting institution group does not prevent unrelated valid institution groups from being validated"
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

                    Expect.equal
                        result.Errors
                        [
                            {
                                Entity = InstitutionKey SyntheticInstitution.bank01.InstitutionId
                                Issue = ConflictingInstitutionAttributes
                            }
                        ]
                        (ValidationReport.formatErrors result.Errors)

                    let validIds =
                        result.Valid
                        |> List.map (fun i -> i.InstitutionId)
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