namespace AMLGraph.Tests.TestData

open AMLGraph.Domain

module SyntheticCustomer =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let john =
        {
            CustomerId = CustomerId "SYN-C001"
            FirstName = "John"
            LastName = "Smith"
            DOB = "1970-01-01"
            Occupation = "Teacher"
            RiskRating = 2
        }

    let johnDifferentOccupation =
        {
            john with
                Occupation = "Engineer"
        }

    let johnHigherRisk =
        {
            john with
                RiskRating = 5
        }

    let mary =
        {
            CustomerId = CustomerId "SYN-C002"
            FirstName = "Mary"
            LastName = "Jones"
            DOB = "1980-05-12"
            Occupation = "Nurse"
            RiskRating = 1
        }