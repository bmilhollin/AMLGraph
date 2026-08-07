namespace AMLGraph.SyntheticData

open AMLGraph.Domain

module SyntheticAccount =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let a100 =
        {
            AccountId = AccountId "SYN-A001"
            InstitutionId = InstitutionId "SYN-FI001"
            AccountType = Checking
            OpenDate = "2020-01-01"
            Balance = 1000.00m  // m converts to decimal, money
        }

    let a100DifferentInstitution =
        {
            AccountId = AccountId "SYN-A001"
            InstitutionId = InstitutionId "SYN-FI002"
            AccountType = Checking
            OpenDate = "2020-01-01"
            Balance = 1000.00m
        }

    let a100DifferentBalance =
        {
            a100 with
                Balance = 1500.00m
        }

    let a200 = 
        {
            AccountId = AccountId "SYN-A002"
            InstitutionId = InstitutionId "SYN-FI001"
            AccountType = Savings
            OpenDate = "2020-01-01"
            Balance = 2000.00m
        }

    let a300 = 
        {
            AccountId = AccountId "SYN-A003"
            InstitutionId = InstitutionId "SYN-FI001"
            AccountType = CreditCard
            OpenDate = "2020-01-01"
            Balance = 500.00m
        }

    let a400 = 
        {
            AccountId = AccountId "SYN-A004"
            InstitutionId = InstitutionId "SYN-FI004"
            AccountType = CreditCard
            OpenDate = "2022-01-01"
            Balance = 5000.00m
        }