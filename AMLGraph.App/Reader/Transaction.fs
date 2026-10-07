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
                    match TryParse.currency(fields[7].Trim()) with
                    | Some currency ->
                        currency
                    | None ->
                        failwith
                            $"Invalid from currency code '{fields[7]}'. Expected a valid currency code."

                let toAmount =
                    fields[8].Trim()
                    |> decimal

                let toCurrency =
                    match TryParse.currency(fields[9].Trim()) with
                    | Some currency ->
                        currency
                    | None ->
                        failwith
                            $"Invalid to currency code '{fields[9]}'. Expected a valid currency code."

                let format =
                    match TryParse.paymentFormat(fields[10].Trim()) with
                    | Some format ->
                        format
                    | None ->
                        failwith
                            $"Invalid payment format '{fields[10]}'. Expected a valid payment format."
                    
                yield
                    {
                        TransactionId = transactionId
                        Timestamp = timestamp
                        FromAccount = UniqueAccountId (fromAccountId, fromInstitutionId)
                        ToAccount = UniqueAccountId (toAccountId, toInstitutionId)
                        Sent = {Amount = fromAmount; Currency = fromCurrency}
                        Received = {Amount = toAmount; Currency = toCurrency}
                        Format = format
                    }                   
        }
        |> Seq.toList