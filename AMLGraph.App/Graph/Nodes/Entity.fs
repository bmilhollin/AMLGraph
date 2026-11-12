namespace AMLGraph.Graph.Nodes

open AMLGraph.Domain
open AMLGraph.Infrastructure

module Entity =

    let private toParameters (entity: 2 thinkEntity) =
        dict [
            "entityId", box (Unwrap.entityId entity.EntityId)
            "entityType", box (Parse.entityTypeString entity.EntityType)
            "name", box entity.Name
        ]

    let create (entities: Entity list) =

        let cypher =
            """
            MERGE (e:Entity {entityId:$entityId})
            SET
                e.entityType = $entityType,
                e.name = $name
            """

        async {
            for entity in entities do

                do!
                    Neo4j.executeWriteAsync
                        cypher
                        (toParameters entity)

            printfn "Entity nodes created"
        }
            