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
                    match Parse.tryParseCountryCode(fields[3].Trim()) with
                    | Some countryCode ->
                        countryCode
                    | None ->
                        failwith
                            $"Invalid country code '{fields[3]}'. Expected an ISO alpha-2 country code."

                yield
                    {
                        InstitutionId = institutionId
                        Name = name
                        InstitutionType = institutionType
                        CountryCode = countryCode
                    }
        }
        |> Seq.toList