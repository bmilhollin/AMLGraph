namespace AMLGraph.Reporting

open AMLGraph.Domain

module ValidationReport =

    let private formatEntityId entityId =
        sprintf
            "Invalid Entity - %s"
            (Unwrap.entityId entityId)

    let private formatInstitutionId institutionId =
        sprintf
            "Invalid Institution - %s"
            (Unwrap.institutionId institutionId)

    let private formatUniqueAccountId uniqueAccountId =
        let accountId, institutionId =
            Unwrap.uniqueAccountId uniqueAccountId

        sprintf
            "Invalid AccountKey - Account %s / Institution %s"
            (Unwrap.accountId accountId)
            (Unwrap.institutionId institutionId)

    let private formatUniqueTransactionId uniqueTransactionId =
        let transactionId, institutionId =
            Unwrap.uniqueTransactionId uniqueTransactionId

        sprintf
            "Invalid TranactionKey - Transaction %s / Institution %s"
            (Unwrap.transactionId transactionId)
            (Unwrap.institutionId institutionId)

    let private formatEntity entity =
        match entity with
        | EntityKey personId ->
            formatEntityId personId
            
        | InstitutionKey institutionId ->
            formatInstitutionId institutionId

        | AccountKey accountKey ->
            formatUniqueAccountId accountKey

        | TransactionKey transactionId ->
            formatUniqueTransactionId transactionId

    let private formatIssue issue =
        match issue with
        | ConflictingEntityAttributes ->
            "Multiple records for this entity contain conflicting attributes."

        | ConflictingInstitutionAttributes ->
            "Multiple records for this institution contain conflicting attributes."

        | ConflictingTransactionAttributes ->
            "Multiple records for this transaction contain conflicting attributes."

        | MissingInstitution ->
            "Referenced institution does not exist or failed validation."

        | MissingAccount ->
            "Referenced account does not exist or failed validation."

        | MissingFromAccountId ->
            "Referenced FROM account does not exist or failed validation."

        | MissingToAccountId ->
            "Referenced TO account does not exist or failed validation."

        | MissingFromInstitution ->
            "Referenced FROM institution does not exist or failed validation."

        | MissingToInstitution ->
            "Referenced TO institution does not exist or failed validation."

        | MismatchedInstitutions ->
            "Customer and account belong to different institutions."  // TODO understand how this fits with new IBM domain

    let formatError (error: ValidationError) =
        sprintf
            "%s\n  - %s"
            (formatEntity error.Entity)
            (formatIssue error.Issue)

    let private formatEntityErrors (entity, errors) =
        let formattedIssues =
            errors
            |> List.map (fun error ->
                sprintf "  - %s" (formatIssue error.Issue))
            |> String.concat "\n"

        sprintf
            "%s\n%s"
            (formatEntity entity)
            formattedIssues

    let formatErrors (errors: ValidationError list) =
        errors
        |> List.groupBy (fun error -> error.Entity)
        |> List.map formatEntityErrors
        |> String.concat "\n\n"

    let summarizeErrors (errors: ValidationError list) = 
        "Validation Errors:\n" + formatErrors errors