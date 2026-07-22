namespace AMLGraph.Validation

open AMLGraph.Domain

module Customer =

    let private customersMatch left right =
        left.FirstName = right.FirstName &&
        left.LastName = right.LastName &&
        left.DOB = right.DOB &&
        left.Occupation = right.Occupation &&
        left.RiskRating = right.RiskRating 

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingCustomer customerId =
        {
            Entity = CustomerKey customerId
            Issue = ConflictingCustomerAttributes
        }

    /// A customerId could exist on multiple rows within Customers.tsv.
    /// If a customer has multiple rows and any of the other fields besides customerId are different, 
    /// that customerId is considered a customer with conflicting attributes,
    /// and the customer will not be used in the graph. Conflicted customers are captured for review.
    let validate (customers: Customer list) : Validated<Customer list> =

        let groups =
            customers
            |> List.groupBy (fun a -> a.CustomerId)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        let validCustomers = ResizeArray<Customer>()
        let errors = ResizeArray<ValidationError>()

        for (_, group) in singletonGroups do
            validCustomers.Add(group.Head)

        for (_, group) in duplicateGroups do

            match group with
            | customer :: others ->
                if List.forall (customersMatch customer) others then

                    validCustomers.Add(customer)

                else

                    errors.Add(conflictingCustomer customer.CustomerId)
            | [] ->
                invalidOp "Unexpected empty customer group."


        {
            Valid = List.ofSeq validCustomers
            Errors = List.ofSeq errors
        }