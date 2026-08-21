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

                if fields.Length <> 10 then
                    failwith $"Unexpected transaction record: {line}"

                let timestamp =
                    fields[0].Trim()
                    |> DateTime.Parse

                let fromInstitutionId =
                    fields[1].Trim()
                    |> InstitutionId
                
                let fromAccountId =
                    fields[2].Trim()
                    |> AccountId
                
                let toInstitutionId =
                    fields[3].Trim()
                    |> InstitutionId
                
                let toAccountId =
                    fields[4].Trim()
                    |> AccountId

                let fromAmount =
                    fields[5].Trim()
                    |> decimal

                let fromCurrency =
                    fields[6].Trim()
                    |> Parse.currency

                let toAmount =
                    fields[7].Trim()
                    |> decimal

                let toCurrency =
                    fields[8].Trim()
                    |> Parse.currency
                    
                let format =
                    fields[9].Trim()
                    |> Parse.paymentFormat
                    
                yield
                    {
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