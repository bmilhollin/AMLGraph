namespace AMLGraph.Reader

open System.IO
open AMLGraph.Domain


module Person =

    let read (filePath:string) =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')

                if fields.Length <> 5 then
                    failwith $"Unexpected Entity record: {line}"

                let personId =
                    fields[0].Trim()
                    |> PersonId

                let firstName = fields[1].Trim()

                let lastName = fields[2].Trim()

                let dob = fields[3].Trim()

                let occupation = fields[4].Trim()

                yield
                    {
                        PersonId = personId
                        FirstName = firstName
                        LastName = lastName
                        Dob = dob
                        Occupation = occupation
                    }
            }
        |> Seq.toList