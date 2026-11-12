namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Institution =

    let private toParameters (institution:Institution) =
        dict [
            "institutionId", box (Unwrap.institutionId institution.InstitutionId)
            "name", box institution.Name
        ]

    let create (institutions:Institution list) =

        let cypher =
            """
            MERGE (i:Institution {institutionId:$institutionId})
            SET
                i.name = $name,
            """

        async {
            for institution in institutions do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters institution)

            printfn "Institution nodes created"
        }
            