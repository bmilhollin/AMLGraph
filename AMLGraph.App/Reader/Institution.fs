namespace AMLGraph.Reader

open System.IO
open AMLGraph.Domain


module Institution =

    let read (filePath:string) =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 4 then
                    failwith $"Unexpected Institution record: {line}"

                let institutionId =
                    fields[0].Trim()
                    |> InstitutionId

                let name = fields[1].Trim()

                let institutionType = fields[2].Trim()

                let countryCode = 
                    match fields[3].Trim().Length with
                    | 2 -> fields[3].Trim()
                    | _ -> failwith $"Invalid country code: {fields[3].Trim()}"

                yield
                    {
                        InstitutionId = institutionId
                        Name = name
                        InstitutionType = institutionType
                        CountryCode = countryCode
                    }
        }
        |> Seq.toList