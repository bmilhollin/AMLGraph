namespace AMLGraph.Readers

open System.IO
open AMLGraph.Domain

module Readers =

    let readCustomersFromFile (filePath:string) =

        use reader = new StreamReader(filePath)

        // Read and discard header row
        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                yield
                    {
                        CustomerId = fields[0].Trim()
                        FirstName = fields[1].Trim()
                        LastName = fields[2].Trim()
                        DOB = fields[3].Trim()
                        Occupation = fields[4].Trim()
                        RiskRating = int fields[5]
                    }
        }
        |> Seq.toList
    
    let readAccountsFromFile (filePath:string) : Account list * Ownership list =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore // header

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 6 then
                    failwith $"Unexpected account record: {line}"

                let accountId = fields[0].Trim()
                let customerId = fields[1].Trim()
                let institutionId = fields[2].Trim()
                let accountType = fields[3].Trim()
                let openDate = fields[4].Trim()
                let balance = decimal fields[5]

                yield
                    {
                        AccountId = accountId
                        InstitutionId = institutionId
                        AccountType = accountType
                        OpenDate = openDate
                        Balance = balance
                    },
                    {
                        AccountId = accountId
                        CustomerId = customerId
                    }
        }
        |> Seq.toList
        |> List.unzip