namespace AMLGraph.Validation

open AMLGraph.Domain

module Transacts =

    // Account and Transaction Institutions have been validated
    // in Account and Transaction validatons

    let private missingAccount transactsKey =
        {
            Entity = TransactsKey transactsKey
            Issue = MissingAccount
        }

    let private missingTransaction transactsKey =
        {
            Entity = TransactsKey transactsKey
            Issue = MissingTransaction
        }

    let private mismatchedInstitutions transactsKey=
        
        {
            Entity = TransactsKey transactsKey
            Issue = MismatchedInstitutions
        }

    let private validateTransacts 
        validAccountKeys 
        validTransactionKeys 
        transacts =

        let accountExists =
            Set.contains transacts.AccountId validAccountKeys

        let transactionExists =
            Set.contains transacts.TransactionId validTransactionKeys

        let accountInstitutionId =
                snd (EntityIds.uniqueAccountIdValue transacts.AccountId)

        let transactionInstitutionId =
                snd (EntityIds.uniqueTransactionIdValue transacts.TransactionId)

        let errors =
            [
                if not accountExists then
                    missingAccount transacts.Key

                if not transactionExists then
                    missingTransaction transacts.Key

                if accountInstitutionId <> transactionInstitutionId then
                    mismatchedInstitutions transacts.Key
            ]

        if List.isEmpty errors then
            Some transacts, []
        else
            None, errors

    let validate 
        (accounts: Account list) 
        (transactions: Transaction list)
        (transacts: Transacts list) : Validated<Transacts list> =

        let normalizedTransacts =
            transacts
            |> List.distinct
        
        let validAccountKeys =
            accounts
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validTransactionKeys =
            transactions
            |> List.map (fun x -> x.Key)
            |> Set.ofList

        let validTransacts = ResizeArray<Transacts>()
        let errors = ResizeArray<ValidationError>()

        for transacts in normalizedTransacts do

            let validTransact, validationErrors =
                validateTransacts validAccountKeys validTransactionKeys transacts

            match validTransact with
            | Some transacts ->
                validTransacts.Add transacts
            | None ->
                ()

            errors.AddRange validationErrors

        {
            Valid = List.ofSeq validTransacts
            Errors = List.ofSeq errors
        }