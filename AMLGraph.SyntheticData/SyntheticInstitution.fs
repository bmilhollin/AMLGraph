namespace AMLGraph.SyntheticData

open AMLGraph.Domain

module SyntheticInstitution =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let bank01 =
        {
            InstitutionId = InstitutionId "SYN-FI001"
            Name = "First National Bank"
            InstitutionType = "Bank"
            CountryCode = "US"
        }

    let bank01DifferentCountryCode =
        {
            bank01 with
                CountryCode = "GB"
        }

    let bank02 =
        {
            InstitutionId = InstitutionId "SYN-FI002"
            Name = "Community Bank"
            InstitutionType = "Bank"
            CountryCode = "US"
        }

    let bank03 =
        {
            InstitutionId = InstitutionId "SYN-FI003"
            Name = "Acme Bank"
            InstitutionType = "Bank"
            CountryCode = "DE"
        }

