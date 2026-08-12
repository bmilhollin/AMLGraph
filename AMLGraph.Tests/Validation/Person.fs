namespace AMLGraph.Tests.Validation

open Expecto

open AMLGraph.Domain
open AMLGraph.Validation
open AMLGraph.SyntheticData

module Person =

    [<Tests>]
    let tests =

        testList "Person Validation" [

            testCase 
                "A single person with unique personId is valid"
                (fun () ->

                    // Arrange
                    let persons =
                        [
                            SyntheticPerson.john
                        ]
                    
                    // Act
                    let result =
                        Person.validate persons

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid person"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase 
                "Duplicate personIds with identical attributes produce one valid person" 
                (fun () ->

                    // Arrange
                    let persons =
                        [
                            SyntheticPerson.john
                            SyntheticPerson.john
                        ]

                    
                    // Act
                    let result =
                        Person.validate persons

                    // Assert
                    Expect.equal
                        result.Valid.Length
                        1
                        "Expected 1 valid person"

                    Expect.isEmpty
                        result.Errors
                        "Expected 0 validation errors"
                )

            testCase
                "Duplicate personIds with conflicting attributes are rejected"
                (fun () ->
                    // Arrange
                    let persons =
                        [
                            SyntheticPerson.john
                            SyntheticPerson.johnDifferentOccupation
                        ]

                    // Act
                    let result =
                        Person.validate persons
                        
                    // Assert
                    Expect.isEmpty
                        result.Valid
                        "Expected 0 valid persons"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingPersonAttributes
                        "Expected conflicting person attributes error"

                    Expect.equal
                        error.Entity
                        (PersonKey SyntheticPerson.john.PersonId)
                        "Expected error to reference the conflicting person"
                    )

            testCase
                "Conflicting personIds groups do not prevent valid person groups from being imported"
                (fun () ->
                    // Arrange
                    let persons =
                        [
                            SyntheticPerson.john
                            SyntheticPerson.johnDifferentOccupation
                            SyntheticPerson.mary
                            SyntheticPerson.james
                        ]

                    // Act
                    let result =
                        Person.validate persons
                        
                    // Assert
                    Expect.hasLength
                        result.Valid
                        2
                        "Expected 2 valid persons"

                    Expect.hasLength
                        result.Errors
                        1
                        "Expected 1 validation error"

                    let error = result.Errors.Head

                    Expect.equal
                        error.Issue
                        ConflictingPersonAttributes
                        "Expected conflicting person attributes error"

                    Expect.equal
                        error.Entity
                        (PersonKey SyntheticPerson.john.PersonId)
                        "Expected error to reference the conflicting person"

                    let validIds =
                        result.Valid
                        |> List.map (fun p -> p.PersonId)
                        |> Set.ofList

                    Expect.equal
                        validIds
                        (   
                            set [
                                    SyntheticPerson.mary.PersonId
                                    SyntheticPerson.james.PersonId
                                ]
                        )
                        "Expected mary and james to be valid persons"
                )
        ]