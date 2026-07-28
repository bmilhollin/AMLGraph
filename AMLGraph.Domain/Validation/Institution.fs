namespace AMLGraph.Validation

open AMLGraph.Domain

module Institution =

    let private institutionAttributesMatch left right =
        left.Name = right.Name &&
        left.InstitutionType = right.InstitutionType &&
        left.CountryCode = right.CountryCode 

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingInstitution institutionId =
        {
            Entity = InstitutionKey institutionId
            Issue = ConflictingInstitutionAttributes
        }

    /// An institutionId could exist on multiple rows within institutions.tsv.
    /// If a institution has multiple rows and any of the other fields besides institutionId are different, 
    /// that institutionId is considered a institution with conflicting attributes,
    /// and the institution will not be used in the graph. Conflicted institutions are captured for review.
    let validate (institutions: Institution list) : Validated<Institution list> =

        let groups =
            institutions
            |> List.groupBy (fun a -> a.InstitutionId)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        let validInstitutions = ResizeArray<Institution>()
        let errors = ResizeArray<ValidationError>()

        for (_, group) in singletonGroups do
            validInstitutions.Add(group.Head)

        for (_, group) in duplicateGroups do

            match group with
            | institution :: others ->
                if List.forall (institutionAttributesMatch institution) others then

                    validInstitutions.Add(institution)

                else

                    errors.Add(conflictingInstitution institution.InstitutionId)
            | [] ->
                invalidOp "Unexpected empty institution group."


        {
            Valid = List.ofSeq validInstitutions
            Errors = List.ofSeq errors
        }