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

    let private missingInstitution uniqueCustomerId =
        {
            Entity = CustomerKey uniqueCustomerId
            Issue = MissingInstitution
        }

    /// A customerId/institutionId pair could exist on multiple rows within Customers.tsv.
    /// If a customerId/institutionId pair has multiple rows and any of the other fields are different, 
    /// that customerId/institutionId pair is considered a uniqueCustomerId with conflicting attributes,
    /// and the uniqueCustomerId will not be used in the graph. Conflicted uniqueCustomerId are captured for review.
    let validate 
        (validInstitutions: Set<InstitutionId>) 
        (customers: Customer list) : Validated<Customer list> =

        let validCustomers = ResizeArray<Customer>()
        let errors = ResizeArray<ValidationError>()

        let isValidInstitution (customer: Customer) = 
            if validInstitutions.Contains(customer.InstitutionId) then
                validCustomers.Add(customer)
            else
                errors.Add(missingInstitution customer.Key)

        let groups =
            customers
            |> List.groupBy (fun x -> x.Key)

        let singletonGroups, duplicateGroups =
            groups
            |> List.partition isSingleton

        for (_, group) in singletonGroups do
            isValidInstitution group.Head     

        for (_, group) in duplicateGroups do

            match group with
            | customer :: others ->
                if List.forall (customerAttributesMatch customer) others then

                    isValidInstitution customer

                else

                    errors.Add(conflictingCustomer customer.Key)
                    if not (validInstitutions.Contains(customer.InstitutionId)) then   
                        errors.Add(missingInstitution customer.Key)
            | [] ->
                invalidOp "Unexpected empty customer group."


        {
            Valid = List.ofSeq validCustomers
            Errors = List.ofSeq errors
        }