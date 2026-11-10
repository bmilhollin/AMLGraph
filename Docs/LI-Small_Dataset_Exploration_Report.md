# IBM LI-Small Dataset Exploration Report

**Project:** AMLGraph  
**Dataset:** IBM synthetic AML data, LI-Small account and transaction CSVs  
**Date:** November 10, 2026  
**Companion analysis:** `Explore LI-Small.fsx` (reorganized F# walkthrough)

## Purpose and scope

This report records observations from exploring the **LI-Small** account and transaction files before aligning AMLGraph's domain model with the available IBM data. Its purpose is to distinguish:

- **Observed:** Results of analyses run against the supplied LI-Small CSVs.
- **Interpretation:** Plausible explanations that are *not* established by the data.
- **Design implication:** A proposed consequence for AMLGraph, subject to implementation review.
- **Open question:** A point not resolved by the available files or exploration.

The analyses used account rows with five fields and transaction rows with eleven fields. The companion F# script uses `Split(',')` and retains rows with the expected number of fields; the reported counts therefore describe the **successfully parsed rows**, not a separate audit of malformed CSV records. Results were reported from local execution; the CSVs were not independently rerun in preparing this document.

## Executive summary

| Measure | Observed result |
|---|---:|
| Account reference records / distinct account keys | 712,688 |
| Distinct entities | 224,931 |
| Valid 11-field transaction records | 6,924,049 |
| Accounts appearing in at least one transaction | 705,907 (99.05%) |
| Accounts with no observed transactions | 6,781 (0.95%) |
| Transactions flagged as laundering | 3,565 (0.0515%) |
| Self-transactions (same sending and receiving account) | 804,477 (11.62%) |
| Reinvestment transactions | 650,458 |

The key findings for domain alignment are that **BankId**, not bank name, identifies an institution; **EntityId**, not entity name, identifies an entity; an account is identified by **(BankId, AccountId)**; and accounts should be loaded from reference data rather than inferred only from transactions. The dataset contains no separate customer identifier, customer risk rating, or explicit ownership graph.

## 1. Account reference data

The account file supplies five columns:

| Column | Meaning in this exploration |
|---|---|
| `Bank Name` | Institution display name |
| `Bank ID` | Institution identifier |
| `Account Number` | Account identifier within a bank |
| `Entity ID` | Identifier of the entity associated with the account |
| `Entity Name` | Entity display name, including a type-like prefix |

**Observed:** 712,688 account rows were retained. There are 712,688 distinct `(BankId, AccountId)` keys, so no duplicate account keys were found in the parsed reference data.

**Design implication:** Retain all five source fields during ingestion. Distinguish identifiers from descriptive names.

## 2. Institution identity and bank naming

**Observed:**

- No `BankId` is associated with more than one `BankName` in the reference data.
- **468 bank names** are each associated with multiple bank IDs.
- Numbered bank names (names containing `#`): **27,184 distinct names** and **27,184 distinct bank IDs**.
- Unnumbered bank names: **468 distinct names** and **14,631 distinct bank IDs**.
- None of the multiple-ID bank names contained the substring ` Bank #` in the specific script check.

Examples observed during exploration include numbered names such as `China Bank #2820` and unnumbered names such as `National Bank of Newport`.

**Interpretation:** The numbered and unnumbered naming patterns may reflect different conventions in the simulator. The generator's naming rules have **not** been established.

**Design implication:** Identify an `Institution` by `BankId`. A bank name is not a reliable unique key.

## 3. Entity identity and types

**Observed:** There are **224,931 distinct entities**. The reference data showed neither multiple IDs for a single entity name nor multiple names for a single entity ID. This one-to-one mapping is an observation about LI-Small, **not** a reason to treat names as permanent identifiers.

The entity type is **inferred from the prefix of `EntityName`** before ` #`; it is not supplied as an independent type column.

| Inferred entity type | Distinct entities |
|---|---:|
| Sole Proprietorship | 75,113 |
| Partnership | 74,910 |
| Corporation | 74,761 |
| Individual | 147 |
| **Total** | **224,931** |

All observed entity names matched one of these four prefixes.

**Design implication:** Use `EntityId` for identity. The four observed categories support an `EntityType` discriminated union: `Individual`, `SoleProprietorship`, `Partnership`, and `Corporation`. The inference from names should be explicit at the ingestion boundary.

**Open question:** Why are only **147** entities labeled `Individual`? The files do not explain this imbalance.

## 4. Accounts and entity relationships

**Observed:**

- No `(BankId, AccountId)` key was associated with more than one `EntityId`.
- **79,583** entities have multiple account records.
- **79,423** entities have accounts at multiple banks.

The data supports a relationship in which each observed account belongs to one entity and one institution, while an entity may hold multiple accounts across institutions.

**Design implication:** Model `Account` with the compound key `(BankId, AccountId)` and references to `Entity` and `Institution`. The IBM account and transaction CSVs do **not** provide the `CustomerId`, `RiskRating`, or customer-to-person mapping previously represented by AMLGraph's `Customer` domain type. For an IBM-aligned model, **remove `Customer` rather than inventing missing attributes**.

**Scope limitation:** The richer simulator description discusses ownership relationships, but the explored CSVs do not expose a company ownership table or explicit ownership edges. Do not fabricate `Ownership` relationships from these files.

## 5. Transaction structure and referential integrity

The transaction file contains eleven fields:

| Index | Column |
|---:|---|
| 0 | Timestamp |
| 1 | From Bank |
| 2 | From Account |
| 3 | To Bank |
| 4 | To Account |
| 5 | Amount Received |
| 6 | Receiving Currency |
| 7 | Amount Paid |
| 8 | Payment Currency |
| 9 | Payment Format |
| 10 | Is Laundering |

**Observed:** **6,924,049** rows had eleven fields.

### BankId formatting discrepancy

A direct join of transaction account keys to account-reference keys initially failed for **every** transaction endpoint:

| Check | Missing references |
|---|---:|
| Sending account, before normalization | 6,924,049 |
| Receiving account, before normalization | 6,924,049 |
| Sending account, after normalization | 0 |
| Receiving account, after normalization | 0 |

The discrepancy was **leading zeros in `BankId`**. Normalization trims whitespace and leading zeroes, using `"0"` if the remaining string is empty. This preserved all **712,688** distinct reference account keys.

**Design implication:** Apply consistent BankId normalization when ingesting both files, before forming or comparing `(BankId, AccountId)` keys. Do **not** strip leading zeros from `AccountId` without separate evidence. Treat this as an IBM LI-Small data-ingestion rule, not a universal rule about bank identifiers.

**Scope limitation:** Referential integrity was verified for the retained eleven-field transaction rows.

## 6. Account participation

| Participation | Accounts | Share of accounts |
|---|---:|---:|
| Appears as sender or receiver | 705,907 | 99.05% |
| Does not appear in any observed transaction | 6,781 | 0.95% |
| **Total** | **712,688** | **100%** |

**Observed:** Every transaction endpoint resolves to a reference account after BankId normalization, but **6,781 reference accounts** are not present in the transaction file.

**Design implication:** Create account nodes from the **account reference file**, independently of transactions. Otherwise those 6,781 accounts would be omitted.

**Terminology caution:** “No observed transactions” does not establish that an account is inactive outside this dataset.

## 7. Payment formats and currencies

### Payment formats

| Payment format | Transactions |
|---|---:|
| Cheque | 2,503,158 |
| Credit Card | 1,780,389 |
| ACH | 796,581 |
| Cash | 655,688 |
| Reinvestment | 650,458 |
| Bitcoin | 309,208 |
| Wire | 228,567 |
| **Total** | **6,924,049** |

**Observed:** There are **seven** payment-format values. `Bitcoin` appears both as a payment format and as a currency, so the two fields represent different concepts.

### Currencies

Both the payment and receiving currency fields contain the same **15 distinct currency names**, though their frequencies differ.

| Currency | Payment side | Receiving side |
|---|---:|---:|
| US Dollar | 2,553,887 | 2,537,242 |
| Euro | 1,595,859 | 1,596,407 |
| Yuan | 483,603 | 474,978 |
| Rupee | 340,641 | 344,237 |
| Bitcoin | 309,240 | 313,196 |
| Saudi Riyal | 257,948 | 261,882 |
| Australian Dollar | 211,155 | 213,905 |
| Yen | 210,125 | 211,631 |
| Brazil Real | 199,840 | 202,717 |
| Canadian Dollar | 176,069 | 177,966 |
| Shekel | 174,530 | 177,298 |
| Swiss Franc | 138,251 | 140,076 |
| UK Pound | 99,668 | 98,000 |
| Ruble | 88,492 | 89,140 |
| Mexican Peso | 84,741 | 85,374 |

**Design implication:** Preserve `PaymentFormat`, `PaymentCurrency`, `ReceivingCurrency`, `AmountPaid`, and `AmountReceived` as distinct source concepts. Do not collapse the two amounts or currencies merely because they often match.

## 8. Transaction characteristics

| Characteristic | Transactions | Share |
|---|---:|---:|
| Self-transaction: same normalized sending and receiving account | 804,477 | 11.62% |
| Cross-bank: different normalized BankIds | 6,012,046 | 86.83% |
| Cross-currency: payment and receiving currencies differ | 98,876 | 1.43% |

These categories are **not mutually exclusive**.

### Self-transactions by payment format

| Payment format | Self-transactions |
|---|---:|
| Reinvestment | 650,458 |
| ACH | 96,141 |
| Bitcoin | 44,982 |
| Cheque | 6,341 |
| Credit Card | 4,308 |
| Cash | 1,471 |
| Wire | 776 |
| **Total** | **804,477** |

**Observed:** Reinvestment accounts for approximately **80.9%** of all self-transactions. The remaining **154,019** self-transactions use other formats.

**Design implication:** A transaction with the same sending and receiving account is valid in the observed dataset. Do not reject such records solely because the endpoints match. AMLGraph's `Account → Transaction → Account` representation can express this pattern without a special graph structure.

**Open question:** What do self-transactions in non-Reinvestment formats represent?

## 9. Focused investigation: Reinvestment

After the initial walkthrough, five Reinvestment rows were inspected directly. They had the same sending and receiving account, matching amounts and currencies, and `Is Laundering = 0`. One example was:

```text
Timestamp:          2022/09/01 00:08
From Bank/Account:  011 / 8000ECA90
To Bank/Account:    011 / 8000ECA90
Amount Received:    3195403.00 US Dollar
Amount Paid:        3195403.00 US Dollar
Payment Format:     Reinvestment
Is Laundering:      0
```

A subsequent check across **all 650,458 Reinvestment records** found:

| Test | Exceptions |
|---|---:|
| Sending account differs from receiving account | 0 (established in self-transaction analysis) |
| Amount received differs from amount paid | 0 |
| Receiving currency differs from payment currency | 0 |
| `Is Laundering = 1` | 0 |

**Observed conclusion:** In LI-Small, every Reinvestment transaction is a **same-account, same-amount, same-currency, non-laundering** record.

**Interpretation, not established fact:** These could encode internal investment-related activity, but the CSVs do not identify interest, dividends, principal, investment positions, or account balance changes. In particular, the records **do not establish that Reinvestment means an interest payment**. Treating a same-account record as a literal debit-and-credit transfer would imply no net account-to-account movement; the simulator's intended economic meaning remains unclear.

**External context:** A user-submitted issue in IBM's `AML-Data` GitHub repository reports that Reinvestment transactions in another examined dataset were self-transactions. This corroborates the *pattern* but does not define the term or establish IBM's intent. See [IBM/AML-Data, issue #1](https://github.com/IBM/AML-Data/issues/1).

**Design implication:** Preserve `Reinvestment` as a source payment format; retain the records and their original endpoints and amounts. Do not invent an interest-payment event type or an investment entity on this evidence.

## 10. Laundering ground truth

| `Is Laundering` | Transactions | Share |
|---|---:|---:|
| `0` | 6,920,484 | 99.9485% |
| `1` | 3,565 | 0.0515% |
| **Total** | **6,924,049** | **100%** |

**Observed:** The field is binary in the parsed dataset and extremely imbalanced.

**Design implication:** A Boolean laundering flag fits the observed source values. Preserve it as simulator-provided ground truth rather than a model prediction.

**Analytical implication:** Overall classification accuracy alone would be misleading. Future detection experiments should examine measures such as precision, recall, false-positive burden, and threshold-dependent performance. No detection algorithm or model evaluation was performed as part of this exploration.

## 11. Proposed IBM-aligned AMLGraph domain

The following is a **design proposal informed by the exploration**, not a claim that the CSV itself specifies a software domain model.

| Domain concept | Identity / key | Evidence and treatment |
|---|---|---|
| `Institution` | `BankId` | Bank names are not unique |
| `Entity` | `EntityId` | Four name-derived entity categories observed |
| `Account` | `(BankId, AccountId)` | One observed entity and institution per account |
| `Transaction` | Existing AMLGraph transaction identity requires separate review | Eleven source fields; source does not provide a standalone transaction ID in this file |
| `Customer` | — | Remove from IBM-aligned model: required attributes are absent |
| `Ownership` | — | Do not construct ownership edges without ownership data |

Retain the transaction-node graph pattern:

```text
(Account) -[:SENT]-> (Transaction) -[:RECEIVED_BY]-> (Account)
```

For Reinvestment, both account endpoints may refer to **the same Account node**.

**Important outstanding design question:** The transaction CSV has no explicit transaction ID column. AMLGraph's existing `TransactionId` / transaction-key strategy must be reviewed separately; this exploration does not establish a replacement key or deduplication rule.

## 12. Remaining questions and limits

1. **Reinvestment meaning:** Does it represent interest, proceeds reinvested, or another simulator-specific event? The available data does not say.
2. **Other self-transactions:** Why do 154,019 self-transactions use non-Reinvestment payment formats?
3. **Individual entities:** Why are there only 147 `Individual` entities among 224,931 entities?
4. **Institution naming:** What rules generated the numbered versus unnumbered bank names?
5. **Transaction identity:** How should AMLGraph uniquely identify and distinguish transaction records without a source transaction ID?
6. **Ownership data:** The simulator describes richer ownership relationships, but the explored CSV exports do not provide them.
7. **Parsing and validation:** The walkthrough filters by field count. A production importer should explicitly report malformed records and validate types, timestamps, amounts, currencies, and flags rather than silently filtering rows.

## 13. References and reproducibility

- **Primary evidence:** Locally executed F# exploration of `LI-Small_Accounts.csv` / `LI-Small_Trans.csv`, captured in the companion `Explore LI-Small.fsx` script and subsequent Reinvestment checks. File naming/case may vary in the local working directory.
- **IBM data repository:** https://github.com/IBM/AML-Data
- **IBM presentation consulted during exploration:** Erik Altman, *Synthetic Financial Data — Money Laundering* (February 25, 2021). Describes the simulated financial world and laundering patterns, but does not establish the meaning of the `Reinvestment` payment format in these records.
- **Community issue:** https://github.com/IBM/AML-Data/issues/1 — third-party report of self-transactions; not an authoritative explanation of IBM's transaction semantics.

**Reproducibility note:** The reorganized walkthrough currently reports the principal counts and self-transaction format breakdown. The focused Reinvestment checks (matching amounts, matching currencies, laundering flag) were executed separately during the follow-up investigation and should be incorporated into the companion script if the report is to be fully reproducible from one run.

---

*This report intentionally separates observed LI-Small data behavior from inferred financial meaning and proposed AMLGraph design decisions.*
