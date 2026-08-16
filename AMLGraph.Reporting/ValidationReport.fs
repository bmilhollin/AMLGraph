namespace AMLGraph.Reporting

open AMLGraph.Domain

module ValidationReport =

    let private formatUniqueCustomerId uniqueCustomerId =
        let customerId, institutionId =
            EntityIds.uniqueCustomerIdValues uniqueCustomerId

        sprintf
            "Customer %s / Institution %s"
            (EntityIds.customerIdValue customerId)
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueAccountId uniqueAccountId =
        let accountId, institutionId =
            EntityIds.uniqueAccountIdValues uniqueAccountId

        sprintf
            "Account %s / Institution %s"
            (EntityIds.accountIdValue accountId)
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueTransactionId uniqueTransactionId =
        let transactionId, institutionId =
            EntityIds.uniqueTransactionIdValues uniqueTransactionId

        sprintf
            "Transaction %s / Institution %s"
            (EntityIds.transactionIdValue transactionId)
            (EntityIds.institutionIdValue institutionId)

    let private formatEntity entity =
        match entity with
        | PersonKey personId ->
            sprintf
                "Person %s"
                (EntityIds.personIdValue personId)

        | InstitutionKey institutionId ->
            sprintf
                "Institution %s"
                (EntityIds.institutionIdValue institutionId)

        | CustomerKey customerKey ->
            formatUniqueCustomerId customerKey

        | AccountKey accountKey ->
            formatUniqueAccountId accountKey

        | OwnershipKey ownershipId ->
            let customerKey, accountKey =
                EntityIds.uniqueOwnershipIdValues ownershipId

            sprintf
                "Ownership\n  %s\n  %s"
                (formatUniqueCustomerId customerKey)
                (formatUniqueAccountId accountKey)

        | TransactionKey transactionId ->
            formatUniqueTransactionId transactionId

    let private formatIssue issue =
        match issue with
        | ConflictingPersonAttributes ->
            "Multiple records for this person contain conflicting attributes."

        | ConflictingInstitutionAttributes ->
            "Multiple records for this institution contain conflicting attributes."

        | ConflictingCustomerAttributes ->
            "Multiple records for this customer contain conflicting attributes."

        | ConflictingAccountAttributes ->
            "Multiple records for this account contain conflicting attributes."

        | ConflictingTransactionAttributes ->
            "Multiple records for this transaction contain conflicting attributes."

        | MissingInstitution ->
            "Referenced institution does not exist or failed validation."

        | MissingCustomer ->
            "Referenced customer does not exist or failed validation."

        | MissingAccount ->
            "Referenced account does not exist or failed validation."

        | MismatchedInstitutions ->
            "Customer and account belong to different institutions."

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