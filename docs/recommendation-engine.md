# Recommendation Engine and Merger

This document describes how `RecommendationEngine` turns an `ExecutionPlan` into a ranked list of
`IndexRecommendation`s, and how `RecommendationMerger` consolidates the raw rule output along the way.
It is grounded in `src/SqlIndexAdvisor.Core/Engine/`; if this document and the code disagree, the code
wins and this document needs fixing. The pipeline around the engine is described in
[ARCHITECTURE.md](ARCHITECTURE.md).

## Where it sits

```
ExecutionPlan
    │
    ▼
RecommendationEngine.Analyze(plan)
    │
    ├─ CollectRaw ──► each IIndexRule.Evaluate(plan), in rule order ──► raw List<IndexRecommendation>
    │
    ├─ RecommendationMerger.Merge(raw) ──► consolidated List<IndexRecommendation>
    │
    └─ Rank ──► IReadOnlyList<IndexRecommendation> (confidence desc, impact desc)
```

`RecommendationEngine` is the only public entry point. `RecommendationMerger` is a `public static`
class with a single public method, `Merge`, and is called only from `RecommendationEngine.Analyze`
(and from tests). The CLI never calls the merger directly; it goes through `new RecommendationEngine()`
in `src/SqlIndexAdvisor.Cli/Program.cs`.

## Types involved

| Type | Role |
| --- | --- |
| `IRecommendationEngine` | Interface with one method, `Analyze(ExecutionPlan)`. |
| `RecommendationEngine` | Sealed implementation. Holds an ordered `IReadOnlyList<IIndexRule>`. |
| `IIndexRule` | `Name` plus `Evaluate(ExecutionPlan)`. Rules are independent and must not have side effects. |
| `RecommendationOptions` | Row-count and selectivity thresholds passed to the scan rules through `DefaultRules.All(options)`. |
| `RecommendationMerger` | Static. `Merge(List<IndexRecommendation>)` returns a new list. |
| `IndexRecommendation` | Output record. `Kind` is `CreateIndex` or `SchemaFix`; `Rule` names the producing rule. |
| `Confidence` | `Low`, `Medium`, `High`. Ordered by its integer value, which the merger relies on. |

## Engine construction

There are three constructors:

- `RecommendationEngine()` uses `DefaultRules.All()`, which is `DefaultRules.All(new RecommendationOptions())`.
- `RecommendationEngine(RecommendationOptions)` uses `DefaultRules.All(options)`.
- `RecommendationEngine(IEnumerable<IIndexRule>)` uses the rules given. Null entries are dropped in the
  constructor, so `RegisteredRules` never reports a null rule. The rule order is the order of the
  enumerable, and that order is significant (see below).

The rules wired by `DefaultRules.All` are, in order:

1. `EngineHintRule` (rule name `engine-hint`): replays SQL Server missing-index hints.
2. `FullScanWithFilterRule`: heuristic recommendation for filtered full scans. Takes the options.
3. `ImplicitConversionRule` (rule name `implicit-conversion`): emits `SchemaFix` recommendations.

## Step 1: Collecting raw recommendations

`CollectRaw` walks `_rules` in order. For each rule it calls `Evaluate(plan)` and appends every result to
one flat list. A `null` result from a rule is tolerated and treated as no output. Nothing is de-duplicated
at this stage, so the same index can appear several times, once per rule that suggests it. Exceptions
thrown by a rule are not caught and propagate out of `Analyze`.

Because the list keeps rule order, the first rule's output is always seen first by the merger.

## Step 2: Merging

`RecommendationMerger.Merge` runs in two phases over the raw list.

### Phase A: schema-fix suppression

1. Collect every column name appearing in `KeyColumns` of any `SchemaFix` recommendation into a
   case-insensitive set.
2. Drop every `CreateIndex` recommendation that has at least one key column in that set.
   `SchemaFix` recommendations are never dropped by this step.

The reasoning is that an index whose key includes a column wrapped in an implicit conversion cannot
be used as-is, so recommending it would be misleading. The schema fix is what the user should act on
first.

The match is by column name only. It is not qualified by table, so a `SchemaFix` on `Orders.CustomerId`
also suppresses a `CreateIndex` on `Customers` whose key contains a column named `CustomerId`. Treat this
as a known limitation, not an intended cross-table rule.

### Phase B: prefix consolidation

The surviving recommendations are processed in input order into a `kept` list:

1. For each candidate, search `kept` from the start for the first entry that `IsSameOrPrefix` matches.
2. If none matches, append the candidate to `kept`.
3. If one matches at index `i`, replace `kept[i]` with `MergeRecommendations(kept[i], candidate)`.

`IsSameOrPrefix(a, b)` is true when:

- the tables are equal (case-insensitive),
- both have at least one key column (a pair with a zero-length key list never matches), and
- for the first `min(a.Keys, b.Keys)` positions the key columns are equal (case-insensitive).

So `(A, B)` matches `(A)`, `(A, B)` matches `(A, B, C)`, and `(B)` matches neither `(A)` nor `(A, B)`.
A candidate merges into the first `kept` entry it matches. That entry may then have a wider key list, and
later candidates are compared against the wider key.

### Merging two recommendations

`MergeRecommendations(existing, candidate)` produces one new `IndexRecommendation`:

1. **Pick the key winner.** The recommendation with more key columns wins. On a tie the candidate wins.
   The other one is the loser.
2. **Apply the optimizer preference.** A recommendation is "optimizer-native" when its `Rule` is
   `engine-hint`. The rules are then:
   - winner is optimizer-native, loser is not: keep the winner.
   - winner is not, loser is: swap them. The engine hint becomes the winner even if it has fewer key columns.
   - both are optimizer-native: the one with the higher `EstimatedImpactPercent` becomes the winner.
   - neither is: keep the width-based winner.
3. **Build the key and includes.** Table and `KeyColumns` come from the winner. `IncludeColumns` is the
   winner's includes followed by the loser's, minus any column already in the winner's keys, de-duplicated
   case-insensitively.
4. **Build the reasons.** Winner's `Reasons` followed by the loser's, de-duplicated case-insensitively.
   If neither side is optimizer-native, the line
   `Impact estimate is heuristic, not optimizer-reported.` is appended.
5. **Set the scalar fields.**
   - `EstimatedImpactPercent` is the winner's value. It is not the maximum of the two.
   - `Confidence` is the maximum of the two.
   - `Rule` is `winner.Rule ?? loser.Rule`.

The result is a new object. Neither input is mutated.

Recommendations that never match anything pass through `Merge` unchanged. In particular, the
heuristic-impact reason is added only when two recommendations are actually merged.

## Step 3: Ranking

`Rank` orders the merged list by `Confidence` descending, then by `EstimatedImpactPercent` descending.
LINQ `OrderBy` is stable, so recommendations that tie on both keep their merge order. The result is
returned as a read-only list.

## Worked example

Assume the plan triggers these raw recommendations, in this order:

| # | Source | Table | Keys | Includes | Impact | Confidence |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `engine-hint` | `dbo.Orders` | `CustomerId, OrderDate` | `Total` | 62.0 | High |
| 2 | `fullscanwithfilter` (heuristic) | `dbo.Orders` | `CustomerId` | `Status` | 18.5 | Medium |

Phase A removes nothing (no `SchemaFix`). Phase B: #2 matches #1 because `CustomerId` is a shared prefix.
The key winner is #1 (two keys against one), and the optimizer preference agrees. The merged result has
keys `CustomerId, OrderDate`, includes `Total, Status`, impact `62.0` (the winner's), confidence `High`
(the maximum), rule `engine-hint`, and the reasons from both sides. `Status` is included because it is not
a winner key. The heuristic-impact reason is not added, because one side is optimizer-native.

## Invariants

- `Analyze` does not mutate the plan or the rule outputs. `Merge` builds new objects and does not
  modify its input list.
- `Confidence` never decreases through a merge.
- The output of `Analyze` is deterministic for a given rule list and plan, because every step is
  order-based and the sort is stable.
- Each merged recommendation keeps the (de-duplicated) reasons of both inputs, so the "every
  recommendation carries Reasons" rule from `CLAUDE.md` still holds after merging.

## Known gaps

These were observed while writing this document. They describe current behavior and were not changed.

1. **The engine does not use `DefaultRuleSet`.** `RecommendationEngine` builds its default rules from
   `DefaultRules.All`, which has three rules. `DefaultRuleSet.Rules` / `DefaultRuleSet.Create` has six
   and is not called anywhere in the repository. `KeyLookupRule`, `MissingJoinIndexRule`, and
   `ExpensiveSortRule` therefore never run through the engine. `ARCHITECTURE.md` says new rules go in
   `DefaultRuleSet`; for the engine to see them they must go in `DefaultRules.All` instead.
2. **`ARCHITECTURE.md` says impact takes the maximum when merging.** The code takes the winner's
   impact, not the maximum. Confidence is the value that takes the maximum.
3. **Merged recommendations lose three fields.** `MergeRecommendations` builds a new object with only
   `Table`, `KeyColumns`, `IncludeColumns`, `EstimatedImpactPercent`, `Confidence`, `Rule`, and
   `Reasons`. It does not copy `Kind`, `SourceNodeCost`, or `ExistingIndexName`. A merged `SchemaFix`
   would come out as `CreateIndex`, and a merged recommendation would lose its existing index name.
   Today this is latent: `SchemaFix` recommendations are not merged with `CreateIndex` ones in practice,
   and `KeyLookupRule`, the only producer of `ExistingIndexName`, is not registered in the engine.
4. **Schema-fix suppression is by column name only** (see Phase A above).

## Related

- [ARCHITECTURE.md](ARCHITECTURE.md) for the overall pipeline and the layering rule.
- [IndexRecommendation.md](IndexRecommendation.md) for the output type's properties.
- [RecommendationEngineTestsExtensions.md](RecommendationEngineTestsExtensions.md) for the test helpers.
