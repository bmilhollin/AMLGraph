namespace AMLGraph.Reader

open System
open System.IO
open AMLGraph.Domain

module Transaction =

    let read (filePath:string) =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 7 then
                    failwith $"Unexpected transaction record: {line}"

                let transactionId =
                    fields[0].Trim()
                    |> TransactionId

                let institutionId =
                    fields[1].Trim()
                    |> InstitutionId
                
                let accountId =
                    fields[2].Trim()
                    |> AccountId

                let transactionType =
                    // only specified combinations of actions and methods are allowed
                    TransactionType.ofString 
                        (fields[3].Trim())  // action
                        (fields[4].Trim()) // method

                let amount =
                    fields[5].Trim()
                    |> decimal

                let timestamp =
                    fields[6].Trim()
                    |> DateTime.Parse
                    
                yield
                    {
                        TransactionId = transactionId
                        InstitutionId = institutionId
                        TransactionType = transactionType
                        Amount = amount
                        Timestamp = timestamp
                    },
                    {
                        AccountId = UniqueAccountId (accountId, institutionId)
                        TransactionId = UniqueTransactionId (transactionId, institutionId)
                    }
        }
        |> Seq.toList
        |> List.unzip