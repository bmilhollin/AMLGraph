namespace AMLGraph.Validation

open AMLGraph.Domain

module Person =

    let private personAttributesMatch left right =
        left.FirstName = right.FirstName &&
        left.LastName = right.LastName &&
        left.Dob = right.Dob &&
        left.Occupation = right.Occupation

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingPerson personId =
        {
            Entity = PersonKey personId
            Issue = ConflictingPersonAttributes
        }

    /// A personId could exist on multiple rows within persons.tsv.
    /// If a person has multiple rows and any of the other fields besides personId are different, 
    /// that personId is considered a person with conflicting attributes,
    /// and the person will not be used in the graph. Conflicted persons are captured for review.
    /// Later, we may build out the capability to capture aliases and other conflicting data
    /// that can be used of entity resolution
    let validate 
        (persons: Person list) : Validated<Person list> =

        let groups =
            persons
            |> List.groupBy (fun x -> x.PersonId)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        let validPersons = ResizeArray<Person>()
        let errors = ResizeArray<ValidationError>()

        for (_, group) in singletonGroups do
            validPersons.Add(group.Head)

        for (_, group) in duplicateGroups do

            match group with
            | person :: others ->
                if List.forall (personAttributesMatch person) others then

                    validPersons.Add(person)

                else

                    errors.Add(conflictingPerson person.PersonId)
            | [] ->
                invalidOp "Unexpected empty person group."


        {
            Valid = List.ofSeq validPersons
            Errors = List.ofSeq errors
        }