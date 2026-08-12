namespace AMLGraph.SyntheticData

open AMLGraph.Domain

module SyntheticCustomer =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let john =
        {
            CustomerId = CustomerId "SYN-C001"
            InstitutionId = InstitutionId "SYN-FI001"
            PersonId = PersonId "SYN-P001"
            RiskRating = 1
        }
    
    let johnDifferentInstitution =
        {
            john with
                InstitutionId = InstitutionId "SYN-FI002"
        }

    let johnHigherRisk =
        {
            john with
                RiskRating = 10
        }

    let mary =
        {
            CustomerId = CustomerId "SYN-C002"
            InstitutionId = InstitutionId "SYN-FI001"
            PersonId = PersonId "SYN-P002"
            RiskRating = 2
        }

    let james =
        {
            CustomerId = CustomerId "SYN-C003"
            InstitutionId = InstitutionId "SYN-FI003"
            PersonId = PersonId "SYN-P003"
            RiskRating = 3
        }

    