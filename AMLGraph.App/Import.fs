namespace AMLGraph

open AMLGraph.Domain

module Import =

    type ImportResult<'T> =
        {
            Read : int
            Validation : Validated<'T list>
        }

    type ImportResults =
        {
            Persons : ImportResult<Person>
            Institutions : ImportResult<Institution>
            Customers : ImportResult<Customer>
            Accounts : ImportResult<Account>
            Ownerships : ImportResult<Ownership>
            Transactions : ImportResult<Transaction>
            Has_Transactions : ImportResult<Has_Transaction>
            FundsTransactions: ImportResult<FundsTransaction>
        }
        member this.Errors =
            [
                yield! this.Persons.Validation.Errors
                yield! this.Institutions.Validation.Errors
                yield! this.Customers.Validation.Errors
                yield! this.Accounts.Validation.Errors
                yield! this.Ownerships.Validation.Errors
                yield! this.Transactions.Validation.Errors
                yield! this.Has_Transactions.Validation.Errors
                yield! this.FundsTransactions.Validation.Errors
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

        let validInstitutionIds =
            validatedInstitutions.Valid
            |> List.map (fun i -> i.InstitutionId)
            |> Set.ofList

        let customers =
            Reader.Customer.read "Data/Customers.tsv"

        let validatedCustomers =
            Validation.Customer.validate
                validInstitutionIds
                customers

        let accounts, ownerships =
            Reader.Account.read "Data/Accounts.tsv"

        let validatedAccounts =
            Validation.Account.validate
                validInstitutionIds
                accounts

        let validatedOwnerships =
            Validation.Ownership.validate
                validatedCustomers.Valid
                validatedAccounts.Valid
                ownerships
        
        let transactions, has_Transactions =
            Reader.Transaction.read "Data/Transactions.tsv"            

        let validatedTransactions =
            Validation.Transaction.validate
                validInstitutionIds
                transactions

        let validatedHas_Transactions =
            Validation.Has_Transaction.validate
                validatedAccounts.Valid
                validatedTransactions.Valid
                has_Transactions

        let fundsTransactions =
            Reader.FundsTransaction.read "Data/FundsTransactions.tsv"
        
        {
            Persons =
                {
                    Read = persons.Length
                    Validation = validatedPersons
                }

            Institutions =
                {
                    Read = institutions.Length
                    Validation = validatedInstitutions
                }

            Customers =
                {
                    Read = customers.Length
                    Validation = validatedCustomers
                }

            Accounts =
                {
                    Read = accounts.Length
                    Validation = validatedAccounts
                }

            Ownerships =
                {
                    Read = ownerships.Length
                    Validation = validatedOwnerships
                }
            // TODO REMOVE
            Transactions =
                {
                    Read = transactions.Length
                    Validation = validatedTransactions
                }
            // TODO REMOVE
            Has_Transactions =
                {
                    Read = has_Transactions.Length
                    Validation = validatedHas_Transactions
                }

            FundsTransactions =
                {
                    Read = fundsTransactions.Length
                    Validation = 
                        {
                            Valid = []
                            Errors = []
                        }
                }
        }

    let summarize (results: ImportResults) =
        sprintf
            "Import Summary:\n\
            Persons Read: %d, Valid: %d, Errors: %d\n\
            Institutions Read: %d, Valid: %d, Errors: %d\n\
            Customers Read: %d, Valid: %d, Errors: %d\n\
            Accounts Read: %d, Valid: %d, Errors: %d\n\
            Ownerships Read: %d, Valid: %d, Errors: %d\n\
            Transactions Read: %d, Valid: %d, Errors: %d\n\
            Has_Transactions Read: %d, Valid: %d, Errors: %d\n\
            FundsTransactions Read: %d, Valid: %d, Errors: %d"
            results.Persons.Read
            results.Persons.Validation.Valid.Length
            results.Persons.Validation.Errors.Length
            results.Institutions.Read
            results.Institutions.Validation.Valid.Length
            results.Institutions.Validation.Errors.Length
            results.Customers.Read
            results.Customers.Validation.Valid.Length
            results.Customers.Validation.Errors.Length
            results.Accounts.Read
            results.Accounts.Validation.Valid.Length
            results.Accounts.Validation.Errors.Length
            results.Ownerships.Read
            results.Ownerships.Validation.Valid.Length
            results.Ownerships.Validation.Errors.Length
            // TODO REMOVE SECTION - BEGIN
            results.Transactions.Read                                
            results.Transactions.Validation.Valid.Length
            results.Transactions.Validation.Errors.Length
            results.Has_Transactions.Read
            results.Has_Transactions.Validation.Valid.Length
            results.Has_Transactions.Validation.Errors.Length
            // TODO REMOVE SECTION - END
            results.FundsTransactions.Read                                
            results.FundsTransactions.Validation.Valid.Length
            results.FundsTransactions.Validation.Errors.Length
