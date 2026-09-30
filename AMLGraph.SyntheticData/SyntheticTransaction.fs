namespace AMLGraph.SyntheticData

open System
open AMLGraph.Domain

module SyntheticTransaction =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let existingAccounts  = 
        [
            SyntheticAccount.a100                       // SYN-A001, SYN-FI001
            SyntheticAccount.a100DifferentInstitution   // SYN-A001, SYN-FI002
            SyntheticAccount.a200                       // SYN-A002, SYN-FI001
            SyntheticAccount.a500                       // SYN-A003, SYN-FI003
        ]
    
    let existingInstitutions = 
        existingAccounts 
        |> List.map (fun acc -> acc.InstitutionId)
        |> Set.ofList

    let existingAccountKeys = 
        existingAccounts 
        |> List.map (fun acc -> acc.Key)
        |> Set.ofList

    let t100 = // valid, same institution
        {
            TransactionId = TransactionId "SYN-T001"
            Timestamp = DateTime(2026, 8, 16, 10, 0, 0)
            FromAccount = SyntheticAccount.a100.Key
            ToAccount = SyntheticAccount.a200.Key
            Sent = { Amount = 1000.00m; Currency = USD }
            Received = { Amount = 1000.00m; Currency = USD }
            Format = Cheque
        }

    let t100DifferentSendingInstitution =  // will have different UniqueTransactionId
        {
            t100 with
                FromAccount = SyntheticAccount.a100DifferentInstitution.Key
        }

    let t100WithDifferentFormat =
        {
            t100 with
                Format = Wire
        }

    let t200 = // valid, cross-institution
        {
            t100 with
                TransactionId = TransactionId "SYN-T002"
                ToAccount = SyntheticAccount.a500.Key
        }

    let t200InvalidFromAccount = // invalid
        {
            t200 with
                FromAccount = SyntheticAccount.a600WithInvalidInstitution.Key
        }

    let t200InvalidToAccount = // invalid
        {
            t200 with
                ToAccount = SyntheticAccount.a600WithInvalidInstitution.Key
        }
    
    let t200InvalidToAndFromAccount = // invalid
        {
            t200 with
                FromAccount = SyntheticAccount.a600WithInvalidInstitution.Key
                ToAccount = SyntheticAccount.a600WithInvalidInstitution.Key
        }
    