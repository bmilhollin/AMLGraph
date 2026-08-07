namespace AMLGraph.SyntheticData

open AMLGraph.Domain

module SyntheticOwnership =

    // Synthetic test data only.
    // Contains no production records or personally identifiable information.

    let existingCustomers =
        [
            SyntheticCustomer.john
            SyntheticCustomer.mary
            SyntheticCustomer.james
        ]
    let existingAccounts  = 
        [
            SyntheticAccount.a100
            SyntheticAccount.a100DifferentInstitution
            SyntheticAccount.a200
        ]

    let johnOwnsA100 = 
        {
            CustomerId = SyntheticCustomer.john.CustomerId
            AccountKey = SyntheticAccount.a100.Key
        }

    let johnOwnsA100DifferentInstitution = 
        {
            CustomerId = SyntheticCustomer.john.CustomerId
            AccountKey = SyntheticAccount.a100DifferentInstitution.Key
        }
        
    let johnOwnsA200 = 
        {
            CustomerId = SyntheticCustomer.john.CustomerId
            AccountKey = SyntheticAccount.a200.Key
        }

    let maryOwnsA100WithJohn = 
        {
            CustomerId = SyntheticCustomer.mary.CustomerId
            AccountKey = SyntheticAccount.a100.Key
        }

    let unknownCustomerOwnsA200 = 
        {
            CustomerId = CustomerId "SYN-999"
            AccountKey = SyntheticAccount.a200.Key
        }

    let jamesOwnsUnknownAccount = 
        {
            CustomerId = SyntheticCustomer.james.CustomerId
            AccountKey = (AccountId "SYN-999", InstitutionId "SYN-999") |> UniqueAccountId
        }

    let unknownCustomerOwnsUnknownAccount =
        {
            CustomerId = CustomerId "SYN-999"
            AccountKey = (AccountId "SYN-999", InstitutionId "SYN-999") |> UniqueAccountId
        }
