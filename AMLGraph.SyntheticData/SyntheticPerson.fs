namespace AMLGraph.SyntheticData

open AMLGraph.Domain

module SyntheticPerson =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let john =
        {
            PersonId = PersonId "SYN-P001"
            FirstName = "John"
            LastName = "Smith"
            Dob = "1970-01-01"
            Occupation = "Teacher"
        }

    let johnDifferentOccupation =
        {
            john with
                Occupation = "Engineer"
        }

    let mary =
        {
            PersonId = PersonId "SYN-P002"
            FirstName = "Mary"
            LastName = "Jones"
            Dob = "1980-05-12"
            Occupation = "Nurse"
        }

    let maryDifferentOccupation =
        {
            mary with
                Occupation = "Accountant"
        }

    let james =
        {
            PersonId = PersonId "SYN-P003"
            FirstName = "James"
            LastName = "Browning"
            Dob = "1985-07-22"
            Occupation = "Software Engineer"
        }

    