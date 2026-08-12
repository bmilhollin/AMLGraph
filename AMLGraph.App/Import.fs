namespace AMLGraph

open AMLGraph.Domain

module Import =

    type ImportResult =
        {
            Persons : Validated<Person list> 
            Institutions : Validated<Institution list>
            Customers : Validated<Customer list>
            Accounts : Validated<Account list>
            Ownerships : Validated<Ownership list>
        }
        member this.Errors =
            [
                yield! this.Persons.Errors
                yield! this.Institutions.Errors
                yield! this.Customers.Errors
                yield! this.Accounts.Errors
                yield! this.Ownerships.Errors
            ]

    let loadAndValidate () =

        let persons = 
            Reader.Person.read "Data/Persons.tsv"

        let validatedPersons =
            Validation.Person.validate persons

        let institutions =
            Reader.Institution.read "Data/Institutions.tsv"

        let validatedInstitutions =
            Validation.Institution.validate institutions

        let customers = 
            Reader.Customer.read "Data/Customers.tsv"

        let validInstitutionIds =
                validatedInstitutions.Valid
                |> List.map (fun i -> i.InstitutionId)
                |> Set.ofList

        let validatedCustomers =
            Validation.Customer.validate validInstitutionIds  customers

        let accounts, ownerships = 
            Reader.Account.read "Data/Accounts.tsv"

        let validatedAccounts =        
            Validation.Account.validate validInstitutionIds accounts

        let validatedOwnerships =
            Validation.Ownership.validate 
                validatedCustomers.Valid
                validatedAccounts.Valid
                ownerships

        {
            Persons = validatedPersons
            Institutions = validatedInstitutions
            Customers = validatedCustomers
            Accounts = validatedAccounts
            Ownerships = validatedOwnerships
        }


        // printfn "Read %d persons" persons.Length
        // printfn "Validated %d persons" validatedPersons.Valid.Length
        // if not validatedPersons.Errors.IsEmpty then
        //     printfn "Found %d person validation errors" validatedPersons.Errors.Length
        // printfn "Read %d institutions" institutions.Length
        // printfn "Validated %d institutions" validatedInstitutions.Valid.Length
        // if not validatedInstitutions.Errors.IsEmpty then
        //     printfn "Found %d institution validation errors" validatedInstitutions.Errors.Length
        // printfn "Read %d customers" customers.Length
        // printfn "Validated %d customers" validatedCustomers.Valid.Length
        // if not validatedCustomers.Errors.IsEmpty then
        //     printfn "Found %d customer validation errors" validatedCustomers.Errors.Length
        // printfn "Read %d accounts" accounts.Length
        // printfn "Read %d ownerships" ownerships.Length
        // printfn "Validated %d accounts" validatedAccounts.Valid.Length
        // if not validatedAccounts.Errors.IsEmpty then
        //     printfn "Found %d account validation errors" validatedAccounts.Errors.Length
        // printfn "Validated %d ownerships" validatedOwnerships.Valid.Length
        // if not validatedOwnerships.Errors.IsEmpty then
        //     printfn "Found %d ownership validation errors" validatedOwnerships.Errors.Length
        // let validationErrors =
        //     [
        //         yield! validatedPersons.Errors
        //         yield! validatedInstitutions.Errors
        //         yield! validatedCustomers.Errors
        //         yield! validatedAccounts.Errors
        //         yield! validatedOwnerships.Errors
        //     ]
        // ValidationReport.formatErrors validationErrors
        // |> printfn "Validation Errors:\n%s"