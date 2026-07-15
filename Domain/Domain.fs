namespace AMLGraph.Domain

// All Graph Entities

type Customer =
    {
        CustomerId: string
        FirstName: string
        LastName: string
        DOB: string
        Occupation: string
        RiskRating: int
    }


type FinancialInstitution =
    {
        InstitutionId: string
        Name: string
        InstitutionType: string
        Country: string
    }


type Account =
    {
        AccountId: string
        CustomerId: string
        InstitutionId: string
        AccountType: string
        OpenDate: string
        Balance: decimal
    }