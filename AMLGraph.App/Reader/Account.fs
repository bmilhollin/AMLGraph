namespace AMLGraph.Reader

open System.IO
open AMLGraph.Domain

// The LI-Small_Accounts.tsv file contains institution, account,
// entity, and ownership information.
// All four collections are constructed in a single pass.

type AccountFileData =
    {
        Institutions: Institution list
        Accounts: AMLGraph.Domain.Account list
        Entities: Entity list
        Ownerships: Ownership list
    }

module Account =

    let read (filePath:string) : AccountFileData =

        use reader = new StreamReader(filePath)

        reader.ReadLine() |> ignore

        let institutions = ResizeArray<Institution>()
        let accounts = ResizeArray<AMLGraph.Domain.Account>()
        let entities = ResizeArray<Entity>()
        let ownerships = ResizeArray<Ownership>()

        while not reader.EndOfStream do

            let line = reader.ReadLine()
            let fields = line.Split('\t')

            if fields.Length <> 5 then
                failwith $"Unexpected account record: {line}"

            let institutionName = fields[0].Trim()
            let institutionId = fields[1].Trim() |> InstitutionId
            let accountId = fields[2].Trim() |> AccountId
            let entityId = fields[3].Trim() |> EntityId
            let entityName = fields[4].Trim()

            let entityType =
                let typeName = (entityName.Split('#')[0]).Trim()

                match Parse.tryParseEntityType typeName with
                | Some entityType ->
                    entityType
                | None ->
                    failwith $"Invalid entity type in '{entityName}'"

            institutions.Add {
                InstitutionId = institutionId
                Name = institutionName
            }

            accounts.Add {
                AccountId = accountId
                InstitutionId = institutionId
            }

            entities.Add {
                EntityId = entityId
                EntityType = entityType
                Name = entityName
            }

            ownerships.Add {
                EntityKey = entityId
                AccountKey = UniqueAccountId (accountId, institutionId)
            }

        {
            Institutions = List.ofSeq institutions
            Accounts = List.ofSeq accounts
            Entities = List.ofSeq entities
            Ownerships = List.ofSeq ownerships
        }