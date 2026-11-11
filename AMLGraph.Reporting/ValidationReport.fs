namespace AMLGraph.Reporting

open AMLGraph.Domain

module ValidationReport =

    let private formatPersonId personId =
        sprintf
            "Invalid Person - %s"
            (Unwrap.personId personId)

    let private formatInstitutionId institutionId =
        sprintf
            "Invalid Institution - %s"
            (Unwrap.institutionId institutionId)

    let private formatUniqueCustomerId uniqueCustomerId =
        let customerId, institutionId =
            Unwrap.uniqueCustomerId uniqueCustomerId

        sprintf
            "Invalid CustomerKey - Customer %s / Institution %s"
            (Unwrap.customerId customerId)
            (Unwrap.institutionId institutionId)

    let private formatUniqueAccountId uniqueAccountId =
        let accountId, institutionId =
            Unwrap.uniqueAccountId uniqueAccountId

        sprintf
            "Invalid AccountKey - Account %s / Institution %s"
            (Unwrap.accountId accountId)
            (Unwrap.institutionId institutionId)

    let private formatUniqueOwnershipId uniqueOwnershipId =
        let customerKey, accountKey =
            Unwrap.uniqueOwnershipId uniqueOwnershipId

        sprintf
            "Invalid OwnershipKey -\nCustomerKey %s\nAccountKey %s"
            (formatUniqueCustomerId customerKey)
            (formatUniqueAccountId accountKey)

    let private formatUniqueTransactionId uniqueTransactionId =
        let transactionId, institutionId =
            Unwrap.uniqueTransactionId uniqueTransactionId

        sprintf
            "Invalid TranactionKey - Transaction %s / Institution %s"
            (Unwrap.transactionId transactionId)
            (Unwrap.institutionId institutionId)

    let private formatEntity entity =
        match entity with
        | PersonKey personId ->
            formatPersonId personId
            
        | InstitutionKey institutionId ->
            formatInstitutionId institutionId

        | CustomerKey customerKey ->
            formatUniqueCustomerId customerKey

        | AccountKey accountKey ->
            formatUniqueAccountId accountKey

        | OwnershipKey ownershipKey ->
            formatUniqueOwnershipId ownershipKey

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

        | MissingFromAccountId ->
            "Referenced FROM account does not exist or failed validation."

        | MissingToAccountId ->
            "Referenced TO account does not exist or failed validation."

        | MissingFromInstitution ->
            "Referenced FROM institution does not exist or failed validation."

        | MissingToInstitution ->
            "Referenced TO institution does not exist or failed validation."

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