# Review the three independent promises

The selection contract has membership, order and payload choice. Review each explicitly. For a request `[B, M, A]` and hydration A/X/B, output B/A satisfies membership and requested order. For duplicate A payloads, checking the ID alone says nothing about which alias or other data survived.

## Debugging answers

UMB-L1 still emits the extra identity, even if it moves to the end. The fix must exclude extras, not merely position them. UMB-L2 changes first-payload semantics to last-payload semantics. UMB-L3 repeats identities because lookup does not consume a match. UMB-L4 changes metadata meaning even though the displayed Items are empty. UMB-L5 has no passing-test evidence until discovery/execution produces actual outcomes.

For UMB-01, an all-missing nonempty search can have empty Items and nonzero Total. Do not confuse it with the controller's empty-search short circuit: the nonempty requested list still reaches hydration. The fixture must distinguish a service returning no IDs from a service returning IDs whose entities are absent.

For UMB-02, combine positive and negative properties. “No output ID is extra” is necessary, but the always-empty implementation satisfies it. Require every available requested identity exactly once, in first-request order, with the first hydrated payload. Qualify hydration-permutation invariance to unique identities or equal duplicate payloads.

For UMB-03, sorting a single hydrated page is not equivalent to globally sorting all results before pagination. A good proposal identifies which ordering promise users need and which service can implement it consistently. Protect other consumers when changing a shared helper's contract.

For UMB-04, a missing count based on distinct identities differs from missing request occurrences. Define the unit before presenting a dashboard. Counters alone cannot tell whether a discrepancy arose from index lag, deletion, permissions or a service defect; further investigation is needed.

## Rubric

| Area | Full-credit evidence |
|---|---|
| Contract | Exact membership, first occurrence, first payload and Total semantics |
| Fixture quality | Minimal cases that distinguish plausible wrong implementations |
| Source connection | Actual helper/controller references, not a copied substitute presented as production |
| Reproduction | Focused project, prerequisites, real discovered test outcomes |
| Limits | Unit behavior, integration, performance and authorization claims separated |

Score each area 0–2: absent, explained, demonstrated. Aim for at least 8/10 with no zero in contract. A worksheet score helps identify misunderstandings; it is not a substitute for running the actual regression checks after a code change.

## Review wording to practice

“The assertion checks returned IDs but duplicate hydrated rows can carry different aliases. If dictionary assignment replaces `TryAdd`, the current test still passes while payload policy changes. Add two rows with one identity and different aliases, and assert the chosen alias.” This comment names a plausible mutation and the observable failure it should produce.

[Capstone](06-capstone.md) · [Index](README.md)
