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

                if fields.Length <> 4 then
                    failwith $"Unexpected customer record: {line}, expected 4 fields but found {fields.Length}"

                yield
                    {
                        CustomerId = fields[0].Trim() |> CustomerId
                        InstitutionId = fields[1].Trim() |> InstitutionId
                        EntityId = fields[2].Trim() |> EntityId
                        RiskRating = int fields[3]
                    }
        }
        |> Seq.toList