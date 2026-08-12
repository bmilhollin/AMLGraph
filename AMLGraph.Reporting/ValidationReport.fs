namespace AMLGraph.Reporting

open AMLGraph.Domain

module ValidationReport =

    let formatError (error: ValidationError) =
        sprintf "%A: %A" error.Entity error.Issue

    let formatErrors errors =

        if List.isEmpty errors then
            "***No validation errors"
        else
            errors
            |> List.map formatError
            |> String.concat "\n"