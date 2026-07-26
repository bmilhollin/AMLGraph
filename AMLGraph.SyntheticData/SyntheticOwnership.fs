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
            SyntheticAccount.a200
        ]

    let johnOwnsA100 = 
        {
            CustomerId = SyntheticCustomer.john.CustomerId
            AccountId = SyntheticAccount.a100.AccountId
        }

    let johnOwnsA200 = 
        {
            CustomerId = SyntheticCustomer.john.CustomerId
            AccountId = SyntheticAccount.a200.AccountId
        }

    let maryOwnsA100WithJohn = 
        {
            CustomerId = SyntheticCustomer.mary.CustomerId
            AccountId = SyntheticAccount.a100.AccountId
        }

    let unknownCustomerOwnsA200 = 
        {
            CustomerId = CustomerId "SYN-999"
            AccountId = SyntheticAccount.a200.AccountId
        }

    let jamesOwnsUnknownAccount = 
        {
            CustomerId = SyntheticCustomer.james.CustomerId
            AccountId = AccountId "SYN-999"
        }

    let unknownCustomerOwnsUnknownAccount =
        {
            CustomerId = CustomerId "SYN-999"
            AccountId = AccountId "SYN-999"
        }
