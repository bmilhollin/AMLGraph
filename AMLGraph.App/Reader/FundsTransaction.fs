namespace AMLGraph.Reader

open System
open System.IO
open AMLGraph.Domain

module FundsTransaction =

    let read (filePath:string) =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 11 then
                    failwith $"Unexpected transaction record: {line}"

                let transactionId =
                    fields[0].Trim()
                    |> TransactionId

                let timestamp =
                    fields[1].Trim()
                    |> DateTime.Parse

                let fromInstitutionId =
                    fields[2].Trim()
                    |> InstitutionId
                
                let fromAccountId =
                    fields[3].Trim()
                    |> AccountId
                
                let toInstitutionId =
                    fields[4].Trim()
                    |> InstitutionId
                
                let toAccountId =
                    fields[5].Trim()
                    |> AccountId

                let fromAmount =
                    fields[6].Trim()
                    |> decimal

                let fromCurrency =
                    fields[7].Trim()
                    |> Parse.currency

                let toAmount =
                    fields[8].Trim()
                    |> decimal

                let toCurrency =
                    fields[9].Trim()
                    |> Parse.currency

                let format =
                    fields[10].Trim()
                    |> Parse.paymentFormat
                    
                yield
                    {
                        TransactionId = transactionId
                        Timestamp = timestamp
                        FromAccount = UniqueAccountId (fromAccountId, fromInstitutionId)
                        ToAccount = UniqueAccountId (toAccountId, toInstitutionId)
                        Paid = {Amount = fromAmount; Currency = fromCurrency}
                        Received = {Amount = toAmount; Currency = toCurrency}
                        Format = format
                    }
                   
        }
        |> Seq.toList
        // |> List.unzip