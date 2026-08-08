namespace AMLGraph.Reader

open System.IO
open AMLGraph.Domain

// The Account.tsv file contains account and ownership information.
// There is no distinct reader for ownership information.
// Ownership information is read in with account information using the account reader.

module Account =

    let read (filePath:string) : Account list * Ownership list =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 6 then
                    failwith $"Unexpected account record: {line}"

                let accountId =
                    fields[0].Trim()
                    |> AccountId

                let customerId =
                    fields[1].Trim()
                    |> CustomerId

                let institutionId =
                    fields[2].Trim()
                    |> InstitutionId

                yield
                    {
                        AccountId = accountId
                        InstitutionId = institutionId
                        AccountType = fields[3].Trim() |> AccountType.ofString
                        OpenDate = fields[4].Trim()
                        Balance = decimal fields[5]
                    },
                    {
                        CustomerKey = (customerId, institutionId) |> UniqueCustomerId
                        AccountKey = (accountId, institutionId) |> UniqueAccountId
                    }
        }
        |> Seq.toList
        |> List.unzip