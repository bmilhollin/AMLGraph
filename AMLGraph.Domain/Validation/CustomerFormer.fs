namespace AMLGraph.Validation

open AMLGraph.Domain

module Customer =

    let private customerAttributesMatch (left: Customer) (right: Customer) =

        left.PersonId = right.PersonId &&
        left.RiskRating = right.RiskRating 

    let private isSingleton (_, group) =
        match group with
        | [_] -> true
        | _ -> false

    let private conflictingCustomer uniqueCustomerId =
        {
            Entity = CustomerKey uniqueCustomerId
            Issue = ConflictingCustomerAttributes
        }

    /// A customerId/institutionId pair could exist on multiple rows within Customers.tsv.
    /// If a customerId/institutionId pair has multiple rows and any of the other fields are different, 
    /// that customerId/institutionId pair is considered a uniqueCustomerId with conflicting attributes,
    /// and the uniqueCustomerId will not be used in the graph. Conflicted uniqueCustomerId are captured for review.
    let validate (customers: Customer list) : Validated<Customer list> =

        let groups =
            customers
            |> List.groupBy (fun a -> a.Key)

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
                if List.forall (customerAttributesMatch customer) others then

                    validCustomers.Add(customer)

                else

                    errors.Add(conflictingCustomer customer.Key)
            | [] ->
                invalidOp "Unexpected empty customer group."


        {
            Valid = List.ofSeq validCustomers
            Errors = List.ofSeq errors
        }