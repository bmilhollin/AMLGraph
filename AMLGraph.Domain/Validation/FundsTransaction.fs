namespace AMLGraph.Validation

open AMLGraph.Domain

module FundsTransaction =

    let private transactionAttributesMatch (left: FundsTransaction) (right: FundsTransaction) =

        left.Timestamp = right.Timestamp &&
        left.FromAccount = right.FromAccount &&
        left.ToAccount = right.ToAccount &&
        left.Paid = right.Paid &&
        left.Received = right.Received &&
        left.Format = right.Format

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingTransaction uniqueTransactionId =
        {
            Entity = TransactionKey uniqueTransactionId
            Issue = ConflictingTransactionAttributes
        }

    let private transactionError uniqueTransactionId validationIssue =
        {
            Entity = TransactionKey uniqueTransactionId
            Issue = validationIssue
        }

    let private validateInstitutions 
        (validInstitutions: Set<InstitutionId>)
        (errors: ResizeArray<ValidationError>)
        (transaction: FundsTransaction) =
        let getInstitutionId (uniqueAccountId: UniqueAccountId) =
            uniqueAccountId
            |> EntityIds.uniqueAccountIdValue
            |> snd

        let fromInstitutionId = getInstitutionId transaction.FromAccount
        let toInstitutionId = getInstitutionId transaction.ToAccount

        let fromIsValid = validInstitutions.Contains fromInstitutionId
        let toIsValid = validInstitutions.Contains toInstitutionId

        if not fromIsValid then
            errors.Add(transactionError transaction.Key MissingFromInstitution)

        if not toIsValid then
            errors.Add(transactionError transaction.Key MissingToInstitution)

        fromIsValid && toIsValid

    let private validateUniqueAccountIds 
        (validUniqueAccountIds: Set<UniqueAccountId>)
        (errors: ResizeArray<ValidationError>)
        (transaction: FundsTransaction) =
        
        let fromIsValid = validUniqueAccountIds.Contains transaction.FromAccount
        let toIsValid = validUniqueAccountIds.Contains transaction.ToAccount

        if not fromIsValid then
            errors.Add(transactionError transaction.Key MissingFromAccount)

        if not toIsValid then
            errors.Add(transactionError transaction.Key MissingToAccount)

        fromIsValid && toIsValid

    let private validateInstitutionsAndUniqueAccountIds 
        (validInstitutions: Set<InstitutionId>)
        (validUniqueAccountIds: Set<UniqueAccountId>)
        (errors: ResizeArray<ValidationError>)
        (transaction: FundsTransaction) =

        let institutionsValid =
            validateInstitutions validInstitutions errors transaction

        let uniqueAccountIdsValid =
            validateUniqueAccountIds validUniqueAccountIds errors transaction

        institutionsValid && uniqueAccountIdsValid

    /// A transactionId/fromInstitutionId pair could exist on multiple rows within Transactions.tsv.
    /// If a transactionId/fromInstitutionId pair has multiple rows and any of the other fields are different,
    /// that transactionId/fromInstitutionId pair is considered a uniqueTransactionId with conflicting attributes,
    /// and the uniqueTransactionId will not be used in the graph. Conflicted uniqueTransactionIds are captured for review.
    let validate
        (validInstitutions: Set<InstitutionId>)
        (validUniqueAccountIds: Set<UniqueAccountId>)
        (transactions: FundsTransaction list) : Validated<FundsTransaction list> =

        let validTransactions = ResizeArray<FundsTransaction>()
        let errors = ResizeArray<ValidationError>()

        let addIfValidInstitutionsAndUniqueAccountIds transaction =
            if validateInstitutionsAndUniqueAccountIds validInstitutions validUniqueAccountIds errors transaction then
                validTransactions.Add transaction

        let groups =
            transactions
            |> List.groupBy (fun x -> x.Key)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        for (_, group) in singletonGroups do
            addIfValidInstitutionsAndUniqueAccountIds group.Head

        for (_, group) in duplicateGroups do

            match group with
            | transaction :: others ->
                if List.forall (transactionAttributesMatch transaction) others then

                    addIfValidInstitutionsAndUniqueAccountIds transaction

                else

                    errors.Add(conflictingTransaction transaction.Key)
                    validateInstitutionsAndUniqueAccountIds validInstitutions validUniqueAccountIds errors transaction
                    |> ignore

            | [] ->
                invalidOp "Unexpected empty transaction group."

        {
            Valid = List.ofSeq validTransactions
            Errors = List.ofSeq errors
        }
