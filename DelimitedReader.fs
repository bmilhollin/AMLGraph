namespace AMLGraph

open System.IO

module DelimitedReader =

    let readCustomersFromFile (filePath:string) =

        use reader = new StreamReader(filePath)

        // Read and discard header row
        reader.ReadLine() |> ignore

        seq {
            while not reader.EndOfStream do

                let line = reader.ReadLine()

                let fields = line.Split('\t')
                printfn "Read line: %A" fields

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