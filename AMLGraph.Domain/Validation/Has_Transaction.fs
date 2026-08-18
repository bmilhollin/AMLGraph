namespace AMLGraph.Validation

open AMLGraph.Domain

module Has_Transaction =

    // Account and Transaction Institutions have been validated
    // in Account and Transaction validatons

    let private missingAccount has_TransactionKey =
        {
            Entity = Has_TransactionKey has_TransactionKey
            Issue = MissingAccount
        }

    let private missingTransaction has_TransactionKey =
        {
            Entity = Has_TransactionKey has_TransactionKey
            Issue = MissingTransaction
        }

    let private mismatchedInstitutions has_TransactionKey=
        
        {
            Entity = Has_TransactionKey has_TransactionKey
            Issue = MismatchedInstitutions
        }

    let private validateHas_Transaction
        validAccountKeys 
        validTransactionKeys 
        has_Transaction =

        let accountExists =
            Set.contains has_Transaction.AccountId validAccountKeys

        let transactionExists =
            Set.contains has_Transaction.TransactionId validTransactionKeys

        let accountInstitutionId =
                snd (EntityIds.uniqueAccountIdValue has_Transaction.AccountId)

        let transactionInstitutionId =
                snd (EntityIds.uniqueTransactionIdValue has_Transaction.TransactionId)

        let errors =
            [
                if not accountExists then
                    missingAccount has_Transaction.Key

                if not transactionExists then
                    missingTransaction has_Transaction.Key

                if accountInstitutionId <> transactionInstitutionId then
                    mismatchedInstitutions has_Transaction.Key
            ]

        if List.isEmpty errors then
            Some has_Transaction, []
        else
            None, errors

    let validate 
        (accounts: Account list) 
        (transactions: Transaction list)
        (has_Transaction: Has_Transaction list) : Validated<Has_Transaction list> =

        let normalizedHas_Transactions =
            has_Transaction
            |> List.distinct
        
        let validAccountKeys =
            accounts
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validTransactionKeys =
            transactions
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validHas_Transactions = ResizeArray<Has_Transaction>()
        let errors = ResizeArray<ValidationError>()

        for has_Transaction in normalizedHas_Transactions do

            let validTransact, validationErrors =
                validateHas_Transaction validAccountKeys validTransactionKeys has_Transaction

            match validTransact with
            | Some has_Transaction ->
                validHas_Transactions.Add has_Transaction
            | None ->
                ()

            errors.AddRange validationErrors

        {
            Valid = List.ofSeq validHas_Transactions
            Errors = List.ofSeq errors
        }