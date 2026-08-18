namespace AMLGraph.Reporting

open AMLGraph.Domain

module ValidationReport =

    let private formatPersonId personId =
        sprintf
            "Invalid Person - %s"
            (EntityIds.personIdValue personId)

    let private formatInstitutionId institutionId =
        sprintf
            "Invalid Institution - %s"
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueCustomerId uniqueCustomerId =
        let customerId, institutionId =
            EntityIds.uniqueCustomerIdValue uniqueCustomerId

        sprintf
            "Invalid CustomerKey - Customer %s / Institution %s"
            (EntityIds.customerIdValue customerId)
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueAccountId uniqueAccountId =
        let accountId, institutionId =
            EntityIds.uniqueAccountIdValue uniqueAccountId

        sprintf
            "Invalid AccountKey - Account %s / Institution %s"
            (EntityIds.accountIdValue accountId)
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueOwnershipId uniqueOwnershipId =
        let customerKey, accountKey =
            EntityIds.uniqueOwnershipIdValue uniqueOwnershipId

        sprintf
            "Invalid OwnershipKey -\nCustomerKey %s\nAccountKey %s"
            (formatUniqueCustomerId customerKey)
            (formatUniqueAccountId accountKey)

    let private formatUniqueTransactionId uniqueTransactionId =
        let transactionId, institutionId =
            EntityIds.uniqueTransactionIdValue uniqueTransactionId

        sprintf
            "Invalid TranactionKey - Transaction %s / Institution %s"
            (EntityIds.transactionIdValue transactionId)
            (EntityIds.institutionIdValue institutionId)

    let private formatUniqueHas_TransactionId uniqueHas_TransactionId =
        let uniqueAccountId, uniqueTransactionId =
            EntityIds.UniqueHas_TransactionIdValue uniqueHas_TransactionId

        let accountId, accountInstitutionId =
            EntityIds.uniqueAccountIdValue uniqueAccountId

        let transactionId, transactionInstitutionId =
            EntityIds.uniqueTransactionIdValue uniqueTransactionId

        sprintf
            "Invalid Has_TransactionKey -\nAccountKey - Account %s / Institution %s\nTransactionKey - Transaction %s / Institution %s"
            (EntityIds.accountIdValue accountId)
            (EntityIds.institutionIdValue accountInstitutionId)
            (EntityIds.transactionIdValue transactionId)
            (EntityIds.institutionIdValue transactionInstitutionId)

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

        | Has_TransactionKey has_TransactionKey ->
            formatUniqueHas_TransactionId has_TransactionKey

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

        | MissingTransaction ->
            "Referenced transaction does not exist or failed validation"

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