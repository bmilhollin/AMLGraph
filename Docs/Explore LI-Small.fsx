// Explore LI-Small.fsx
// Reproducible walkthrough of IBM LI-Small account and transaction data.
// Counts below were observed in the current LI-Small files; rerun to verify.
// Assumes comma-separated fields without embedded commas (same parser as original).

open System
open System.IO

let accountPath = @"C:\LI-Small\LI-Small_accounts.csv"
let transactionPath = @"C:\LI-Small\LI-Small_Trans.csv"

// 1. Load account reference data ONCE: BankName, BankId, AccountId, EntityId, EntityName.
let rows =
    File.ReadLines(accountPath)
    |> Seq.skip 1
    |> Seq.map (fun line -> line.Split(','))
    |> Seq.filter (fun fields -> fields.Length = 5)
    |> Seq.map (fun fields ->
        fields.[0].Trim(), fields.[1].Trim(), fields.[2].Trim(),
        fields.[3].Trim(), fields.[4].Trim())
    |> Seq.toArray

let prefix (name: string) =
    let index = name.IndexOf(" #")
    if index >= 0 then name.Substring(0, index) else name

printfn "\n1. Account reference data"
printfn "  Account records: %d" rows.Length // 712,688

// 2. Institution identity: names are descriptive, IDs identify banks.
let banks =
    rows
    |> Array.map (fun (bankName, bankId, _, _, _) -> bankName, bankId)
    |> Array.distinct

let bankNamesWithMultipleIds =
    banks
    |> Array.groupBy fst
    |> Array.choose (fun (name, entries) ->
        let ids = entries |> Array.map snd |> Array.distinct
        if ids.Length > 1 then Some (name, ids) else None)

let bankIdsWithMultipleNames =
    banks
    |> Array.groupBy snd
    |> Array.choose (fun (id, entries) ->
        let names = entries |> Array.map fst |> Array.distinct
        if names.Length > 1 then Some (id, names) else None)

printfn "\n2. Institution identity"
printfn "  Bank names associated with multiple IDs: %d" bankNamesWithMultipleIds.Length // 468
printfn "  Bank IDs associated with multiple names: %d" bankIdsWithMultipleNames.Length // 0

let numberedBanks, unnumberedBanks =
    banks |> Array.partition (fun (name, _) -> name.Contains("#"))

let printBankCounts label (entries: (string * string) array) =
    printfn "  %s distinct names: %d" label (entries |> Array.map fst |> Array.distinct |> Array.length)
    printfn "  %s distinct IDs: %d" label (entries |> Array.map snd |> Array.distinct |> Array.length)

printfn "\nBank naming conventions:"
printBankCounts "Numbered" numberedBanks // 27,184 names; 27,184 IDs
printBankCounts "Unnumbered" unnumberedBanks // 468 names; 14,631 IDs
printfn "  Multiple-ID names containing ' Bank #': %d"
    (bankNamesWithMultipleIds |> Array.filter (fun (name, _) -> name.Contains(" Bank #")) |> Array.length) // 0

printfn "\nSample numbered bank names:"
numberedBanks |> Array.map fst |> Array.distinct |> Array.truncate 10 |> Array.iter (printfn "  %s")
printfn "\nSample unnumbered bank names:"
unnumberedBanks |> Array.map fst |> Array.distinct |> Array.truncate 10 |> Array.iter (printfn "  %s")

// OBSERVATION: Each BankId maps to one BankName; 468 names map to multiple IDs.
// Numbered bank names are unique in the observed data; unnumbered names are reused.
// INTERPRETATION: Numbered names resemble country/type labels; unnumbered names
// resemble US bank names. The IBM naming algorithm is not established.
// DESIGN: Identify Institution by BankId, not BankName.

// 3. Entity identity and type (type is inferred from the name prefix).
let entities =
    rows
    |> Array.map (fun (_, _, _, entityId, entityName) -> entityId, entityName)
    |> Array.distinct

let entityNamesWithMultipleIds =
    entities
    |> Array.groupBy snd
    |> Array.filter (fun (_, entries) -> entries |> Array.map fst |> Array.distinct |> Array.length > 1)

let entityIdsWithMultipleNames =
    entities
    |> Array.groupBy fst
    |> Array.filter (fun (_, entries) -> entries |> Array.map snd |> Array.distinct |> Array.length > 1)

let expectedEntityTypes =
    set [ "Individual"; "Sole Proprietorship"; "Partnership"; "Corporation" ]

let unexpectedEntityNames =
    entities
    |> Array.filter (fun (_, name) -> not (expectedEntityTypes.Contains(prefix name)))

printfn "\n3. Entity identity and types"
printfn "  Distinct entities: %d" entities.Length // 224,931
printfn "  Entity names associated with multiple IDs: %d" entityNamesWithMultipleIds.Length // 0
printfn "  Entity IDs associated with multiple names: %d" entityIdsWithMultipleNames.Length // 0
printfn "  Entities with unexpected name patterns: %d" unexpectedEntityNames.Length // 0
printfn "  Entity types (distinct entities):"
entities
|> Array.countBy (fun (_, name) -> prefix name)
|> Array.sortByDescending snd
|> Array.iter (fun (entityType, count) -> printfn "    %-25s %d" entityType count)
// Sole Proprietorship 75,113; Partnership 74,910; Corporation 74,761; Individual 147.
// OBSERVATION: Four name patterns cover all entities; EntityId and EntityName
// have a 1:1 mapping in this file. Type is not a separate CSV column.
// DESIGN: EntityId identifies Entity; four-case EntityType fits observed data.
// OPEN QUESTION: Why are only 147 entities named Individual?

// 4. Account ownership and entity participation across institutions.
let distinctAccountKeys =
    rows |> Array.map (fun (_, bankId, accountId, _, _) -> bankId, accountId) |> Set.ofArray

let accountsWithMultipleEntities =
    rows
    |> Array.groupBy (fun (_, bankId, accountId, _, _) -> bankId, accountId)
    |> Array.choose (fun (key, accounts) ->
        let ids = accounts |> Array.map (fun (_, _, _, entityId, _) -> entityId) |> Array.distinct
        if ids.Length > 1 then Some (key, ids) else None)

let accountsByEntity =
    rows |> Array.groupBy (fun (_, _, _, entityId, _) -> entityId)

let entitiesWithMultipleAccounts =
    accountsByEntity |> Array.filter (fun (_, accounts) -> accounts.Length > 1) |> Array.length

let entitiesWithMultipleBanks =
    accountsByEntity
    |> Array.filter (fun (_, accounts) ->
        accounts |> Array.map (fun (_, bankId, _, _, _) -> bankId) |> Array.distinct |> Array.length > 1)
    |> Array.length

printfn "\n4. Account ownership"
printfn "  Distinct (BankId, AccountId) keys: %d" distinctAccountKeys.Count // 712,688
printfn "  Accounts associated with multiple entities: %d" accountsWithMultipleEntities.Length // 0
printfn "  Entities with multiple accounts: %d" entitiesWithMultipleAccounts // 79,583
printfn "  Entities with accounts at multiple banks: %d" entitiesWithMultipleBanks // 79,423

printfn "\nExample entity with accounts at three or more banks:"
match accountsByEntity |> Array.tryFind (fun (_, accounts) ->
    accounts |> Array.map (fun (_, bankId, _, _, _) -> bankId) |> Array.distinct |> Array.length >= 3) with
| Some (entityId, accounts) ->
    printfn "  Entity: %s" entityId
    accounts |> Array.iter (fun (_, bankId, accountId, _, _) ->
        printfn "    Bank: %s  Account: %s" bankId accountId)
| None -> printfn "  No example found"
// OBSERVATION: Each observed account belongs to one entity, while entities can
// hold multiple accounts across institutions.
// DESIGN: Account references Entity and Institution directly; no Customer data
// is supplied by this dataset.

// 5. Load transaction data and reconcile account references.
let transactions =
    File.ReadLines(transactionPath)
    |> Seq.skip 1
    |> Seq.map (fun line -> line.Split(','))
    |> Seq.filter (fun fields -> fields.Length = 11)
    |> Seq.toArray

let fromKeys = transactions |> Array.map (fun fields -> fields.[1].Trim(), fields.[2].Trim())
let toKeys = transactions |> Array.map (fun fields -> fields.[3].Trim(), fields.[4].Trim())

let normalizeBankId (bankId: string) =
    let normalized = bankId.Trim().TrimStart('0')
    if normalized = "" then "0" else normalized

let normalizeKey (bankId, accountId) = normalizeBankId bankId, accountId
let normalizedAccountKeys = distinctAccountKeys |> Set.map normalizeKey
let normalizedFromKeys = fromKeys |> Array.map normalizeKey
let normalizedToKeys = toKeys |> Array.map normalizeKey

let missingFromRaw = fromKeys |> Array.filter (fun key -> not (distinctAccountKeys.Contains key))
let missingToRaw = toKeys |> Array.filter (fun key -> not (distinctAccountKeys.Contains key))
let missingFromNormalized = normalizedFromKeys |> Array.filter (fun key -> not (normalizedAccountKeys.Contains key))
let missingToNormalized = normalizedToKeys |> Array.filter (fun key -> not (normalizedAccountKeys.Contains key))

printfn "\n5. Transaction referential integrity"
printfn "  Transactions: %d" transactions.Length // 6,924,049
printfn "  Missing From before normalization: %d" missingFromRaw.Length // 6,924,049
printfn "  Missing To before normalization: %d" missingToRaw.Length // 6,924,049
printfn "  Account keys before normalization: %d" distinctAccountKeys.Count // 712,688
printfn "  Account keys after normalization: %d" normalizedAccountKeys.Count // 712,688
printfn "  Missing From after normalization: %d" missingFromNormalized.Length // 0
printfn "  Missing To after normalization: %d" missingToNormalized.Length // 0
// OBSERVATION: Leading zeros in BankId differ between the two files.
// Normalization removes all missing references without collapsing account keys.
// DESIGN: Normalize BankId consistently during ingestion; Account key is
// (BankId, AccountId). Only successfully parsed 11-field rows are checked.

// 6. Account participation in the observed transactions.
let activeAccountKeys =
    Array.append normalizedFromKeys normalizedToKeys |> Set.ofArray
let inactiveAccountKeys = Set.difference normalizedAccountKeys activeAccountKeys
printfn "\n6. Account participation"
printfn "  Total accounts: %d" normalizedAccountKeys.Count // 712,688
printfn "  Accounts with transactions: %d" activeAccountKeys.Count // 705,907
printfn "  Accounts without transactions: %d" inactiveAccountKeys.Count // 6,781
// OBSERVATION: 6,781 reference accounts have no transaction in this dataset.
// DESIGN: Load accounts independently from transaction records.

// 7. Transaction payment formats and currencies.
printfn "\n7. Payment formats"
transactions
|> Array.countBy (fun fields -> fields.[9].Trim())
|> Array.sortByDescending snd
|> Array.iter (fun (format, count) -> printfn "  %-25s %d" format count)

printfn "\nPayment currencies"
transactions
|> Array.countBy (fun fields -> fields.[8].Trim())
|> Array.sortByDescending snd
|> Array.iter (fun (currency, count) -> printfn "  %-25s %d" currency count)

printfn "\nReceiving currencies"
transactions
|> Array.countBy (fun fields -> fields.[6].Trim())
|> Array.sortByDescending snd
|> Array.iter (fun (currency, count) -> printfn "  %-25s %d" currency count)
// OBSERVATION: Seven payment formats; both currency columns have the same
// 15 distinct values. Bitcoin appears as a payment format and as a currency.
// DESIGN: Retain separate payment/receiving currencies and payment format.

// 8. Transaction characteristics, including self-transactions.
let pairedKeys = Array.zip normalizedFromKeys normalizedToKeys
let selfTransactions = pairedKeys |> Array.filter (fun (a, b) -> a = b) |> Array.length
let crossBankTransactions =
    pairedKeys |> Array.filter (fun ((fromBank, _), (toBank, _)) -> fromBank <> toBank) |> Array.length
let crossCurrencyTransactions =
    transactions |> Array.filter (fun fields -> fields.[6].Trim() <> fields.[8].Trim()) |> Array.length

printfn "\n8. Transaction characteristics"
printfn "  Self-transactions: %d" selfTransactions // 804,477
printfn "  Cross-bank transactions: %d" crossBankTransactions // 6,012,046
printfn "  Cross-currency transactions: %d" crossCurrencyTransactions // 98,876

let selfTransactionFormats =
    transactions
    |> Array.mapi (fun i fields -> normalizedFromKeys.[i] = normalizedToKeys.[i], fields.[9].Trim())
    |> Array.filter fst
    |> Array.countBy snd
    |> Array.sortByDescending snd

printfn "\nSelf-transaction payment formats"
selfTransactionFormats
|> Array.iter (fun (format, count) -> printfn "  %-25s %d" format count)
// OBSERVATION: All 650,458 Reinvestment transactions are self-transactions;
// 154,019 additional self-transactions have other payment formats.
// DESIGN: Allow transactions whose sending and receiving account are identical.
// OPEN QUESTION: What do non-Reinvestment self-transactions represent?

printfn "\nSample reinvestment transactions:"

transactions
|> Array.filter (fun fields ->
    fields.[9].Trim() = "Reinvestment")
|> Array.truncate 5
|> Array.iter (fun fields ->
    printfn "  %A" fields)

let reinvestments =
    transactions
    |> Array.filter (fun fields ->
        fields.[9].Trim() = "Reinvestment")

let countWhere predicate =
    reinvestments
    |> Array.filter predicate
    |> Array.length

printfn "Reinvestment characteristics:"
printfn "  Total: %d" reinvestments.Length

printfn "  Different amounts: %d"
    (countWhere (fun f -> f.[5].Trim() <> f.[7].Trim()))

printfn "  Different currencies: %d"
    (countWhere (fun f -> f.[6].Trim() <> f.[8].Trim()))

printfn "  Laundering flagged: %d"
    (countWhere (fun f -> f.[10].Trim() = "1"))

// DOMAIN OBSERVATIONS — Reinvestment
//
// All 650,458 Reinvestment transactions:
//   - Have identical sending and receiving accounts
//   - Have identical payment and receiving amounts
//   - Have identical payment and receiving currencies
//   - Are flagged as non-laundering
//
// INTERPRETATION
// Reinvestment appears to be a special transaction category
// representing activity within a single account.
//
// DESIGN IMPLICATION
// Preserve Reinvestment as a valid payment format.
// Do not exclude self-transactions during validation.
// Do not infer an investment-related entity or relationship
// that is not represented in the source data.

// 9. Laundering ground-truth flags.
printfn "\n9. Laundering flags"
transactions
|> Array.countBy (fun fields -> fields.[10].Trim())
|> Array.sortBy fst
|> Array.iter (fun (flag, count) -> printfn "  Is Laundering = %s: %d" flag count)
// OBSERVATION: 6,920,484 non-laundering (0); 3,565 laundering (1),
// approximately 0.0515% of the 6,924,049 transactions.
// DESIGN: Binary laundering flag fits the observed data.
// ANALYTICAL NOTE: Class imbalance makes accuracy alone misleading.
