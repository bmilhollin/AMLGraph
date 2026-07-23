namespace AMLGraph.Reader

open System.IO
open AMLGraph.Domain

module Customer =

    let read (filePath:string) =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 6 then
                    failwith $"Unexpected customer record: {line}"

                yield
                    {
                        CustomerId = fields[0].Trim() |> CustomerId
                        FirstName = fields[1].Trim()
                        LastName = fields[2].Trim()
                        DOB = fields[3].Trim()
                        Occupation = fields[4].Trim()
                        RiskRating = int fields[5]
                    }
        }
        |> Seq.toList