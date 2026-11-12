namespace AMLGraph.Validation

open AMLGraph.Domain

module Institution =

    let private institutionAttributesMatch left right =
        left.Name = right.Name  //&&
        // left.InstitutionType = right.InstitutionType &&
        // left.CountryCode = right.CountryCode 

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingInstitution institutionId =
        {
            Entity = InstitutionKey institutionId
            Issue = ConflictingInstitutionAttributes
        }


    /// Institutions sharing an InstitutionId must have identical names.
    /// Identical duplicates are consolidated into one institution.
    /// Conflicting institutions are excluded and reported for review.
    let validate 
        (institutions: Institution list) : Validated<Institution list> =

        let groups =
            institutions
            |> List.groupBy (fun x -> x.InstitutionId)

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