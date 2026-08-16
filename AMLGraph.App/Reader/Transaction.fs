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

                if fields.Length <> 6 then
                    failwith $"Unexpected transaction record: {line}"

                let transactionId =
                    fields[0].Trim()
                    |> TransactionId

                let institutionId =
                    fields[1].Trim()
                    |> InstitutionId

                let transactionType =
                    TransactionType.ofString 
                        (fields[2].Trim())
                        (fields[3].Trim())

                let amount =
                    fields[4].Trim()
                    |> decimal

                let timestamp =
                    fields[5].Trim()
                    |> DateTime.Parse
                    
                yield
                    {
                        TransactionId = transactionId
                        InstitutionId = institutionId
                        TransactionType = transactionType
                        Amount = amount
                        Timestamp = timestamp
                    }
        }
        |> Seq.toList