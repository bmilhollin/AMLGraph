namespace AMLGraph.Validation

open AMLGraph.Domain

module Transaction =

    let private transactionAttributesMatch (left: Transaction) (right: Transaction) =

        left.TransactionType = right.TransactionType &&
        left.Amount = right.Amount  &&
        left.Timestamp = right.Timestamp

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingTransaction uniqueTransactionId =
        {
            Entity = TransactionKey uniqueTransactionId
            Issue = ConflictingTransactionAttributes
        }

    let private missingInstitution uniqueTransactionId =
        {
            Entity = TransactionKey uniqueTransactionId
            Issue = MissingInstitution
        }

    /// A transactionId/institutionId pair could exist on multiple rows within Transactions.tsv.
    /// If a transactionId/institutionId pair has multiple rows and any of the other fields are different, 
    /// that transactionId/institutionId pair is considered a uniqueTransactionId with conflicting attributes,
    /// and the uniqueTransactionId will not be used in the graph. Conflicted uniqueTransactionId are captured for review.
    let validate 
        (validInstitutions: Set<InstitutionId>) 
        (transactions: Transaction list) : Validated<Transaction list> =

        let validTransactions = ResizeArray<Transaction>()
        let errors = ResizeArray<ValidationError>()

        let isValidInstitution (transaction: Transaction) = 
            if validInstitutions.Contains(transaction.InstitutionId) then
                validTransactions.Add(transaction)
            else
                errors.Add(missingInstitution transaction.Key)

        let groups =
            transactions
            |> List.groupBy (fun x -> x.Key)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        for (_, group) in singletonGroups do
            isValidInstitution group.Head     

        for (_, group) in duplicateGroups do

            match group with
            | transaction :: others ->
                if List.forall (transactionAttributesMatch transaction) others then

                    isValidInstitution transaction

                else

                    errors.Add(conflictingTransaction transaction.Key)
                    if not (validInstitutions.Contains(transaction.InstitutionId)) then   
                        errors.Add(missingInstitution transaction.Key)
            | [] ->
                invalidOp "Unexpected empty transaction group."


        {
            Valid = List.ofSeq validTransactions
            Errors = List.ofSeq errors
        }