namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Person =

    let private toParameters (person:Person) =
        dict [
            "personId", box (EntityIds.personIdValue person.PersonId)
            "firstName", box person.FirstName
            "lastName", box person.LastName
            "dob", box person.Dob
            "occupation", box person.Occupation
        ]

    let create (persons:Person list) =

        let cypher =
            """
            MERGE (i:Person {personId:$personId})
            SET
                i.firstName = $firstName,
                i.lastName = $lastName,
                i.dob = $dob,
                i.occupation = $occupation
            """

        async {
            for person in persons do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters person)

            printfn "Person nodes created"
        }
            