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
            CustomerKey = SyntheticCustomer.john.Key
            AccountKey = SyntheticAccount.a100.Key
        }

    let johnOwnsA100DifferentInstitution = 
        {
            CustomerKey = SyntheticCustomer.john.Key
            AccountKey = SyntheticAccount.a100DifferentInstitution.Key
        }
        
    let johnOwnsA200 = 
        {
            CustomerKey = SyntheticCustomer.john.Key
            AccountKey = SyntheticAccount.a200.Key
        }

    let maryOwnsA100WithJohn = 
        {
            CustomerKey = SyntheticCustomer.mary.Key
            AccountKey = SyntheticAccount.a100.Key
        }

    let unknownCustomerOwnsA200 = 

        {
            CustomerKey = (CustomerId "SYN-C999", InstitutionId "SYN-FI001") |> UniqueCustomerId
            AccountKey = SyntheticAccount.a200.Key
        }

    let jamesOwnsUnknownAccount = 
        {
            CustomerKey = SyntheticCustomer.james.Key
            AccountKey = (AccountId "SYN-999", InstitutionId "SYN-999") |> UniqueAccountId
        }

    let unknownCustomerOwnsUnknownAccount =
        {
            CustomerKey = (CustomerId "SYN-C999", InstitutionId "SYN-FI001") |> UniqueCustomerId
            AccountKey = (AccountId "SYN-999", InstitutionId "SYN-999") |> UniqueAccountId
        }
