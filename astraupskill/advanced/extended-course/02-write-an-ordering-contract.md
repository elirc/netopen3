# 02. Write an ordering contract

Read `OrderByRequestedIds` in [ManagementApiControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs). Its current behavior combines membership selection, duplicate policy and ordering. It returns each requested hydrated identity at most once, in the order of the first matching occurrence in the requested sequence, using the first hydrated entity for duplicate payload keys. Missing and unrequested entities are omitted.

## A sort is not the whole operation

Suppose requested keys are B, A and hydration returns A, X, B. Sorting all hydrated entities by their index in the requested array still leaves X unless it is explicitly filtered. If an unknown key receives index minus one, it may even appear before valid matches. The contract must first decide which identities belong in the output and then which order those selected values follow.

Now suppose requested keys are B, A, B. Returning one payload for every requested occurrence would repeat B. The current helper removes a key from its lookup when it emits it, so later requested duplicates cannot emit again. This is a deliberate uniqueness policy. Another application might preserve repeated positions, but that would be a different contract and require different tests.

## Duplicate payloads require a winner

Hydration can return two objects with the same key but different aliases or other fields. The helper uses TryAdd while building its dictionary, so the first encountered hydrated entity wins. It does not merge fields, prefer the last object, or choose based on version or alias. Make payload differences visible in tests so a wrong winner cannot pass by returning the correct key alone.

For example, hydration contains A with alias alpha-first, B with beta-first, then A with alpha-later. Requested order is B, A. The result uses beta-first followed by alpha-first. A test asserting only identities B and A misses the first-payload rule. The alias assertions in the regression suite are therefore meaningful contract evidence, not incidental decoration.

## Write invariants and examples together

An invariant states a general property: every output key was requested and hydrated; no output key repeats; output order follows first eligible requested occurrence; each selected payload is the first hydrated object for its key. Examples make the boundaries inspectable. Use both. A list of examples without invariants can miss a whole category, while abstract invariants without worked cases can hide ambiguity.

Do not add unsupported input promises. The helper's generic constraint requires IEntity, but that does not itself define a policy for null elements, mutable keys during enumeration or a sequence that throws. Those can be useful future robustness investigations, but the current contract should not silently claim graceful handling unless the code or tests establish it.

## Exercise UM-02A: construct a contract table

Create cases for reversed hydration, extra entity, missing entity, repeated request, duplicate payload, empty requested array, empty hydration and all requested identities missing. Use different aliases for duplicate payloads. For each, write exact output keys and payload labels. Keep search metadata outside this pure-helper table because the helper does not receive Total.

Add a mixed case combining several conditions only after the focused cases are clear. A mixed fixture is useful for composition, but focused cases make failures easier to diagnose. If the mixed result is wrong, the smaller tables help identify which rule was violated.

## Exercise UM-02B: compare alternative policies

Describe three possible duplicate-payload policies: first wins, last wins and reject duplicates. Give one scenario where each might be appropriate. Then state which one this helper implements and why changing it would be a shared behavior change. Do not silently replace TryAdd with an indexer assignment during a performance refactor; that changes the winner.

Similarly compare unique requested identities with preserving repeated positions. A playlist-like application may intentionally repeat an item, while this helper's documented entity response returns each identity once. The correct policy depends on the caller contract. Reuse should follow meaning, not merely matching parameter types.

## Exercise UM-02C: review a proposed shortcut

Someone suggests returning hydrated templates ordered by alias because the UI looks tidy. Write a counterexample where the search sequence intentionally differs from alias order. Explain why the integration layer should preserve the upstream ordering authority. A new client-side sort can be a product feature, but it should be explicit and should not masquerade as preserving search results.

## Review standard

A strong contract names membership, order, request deduplication and payload winner separately. Its fixtures use distinguishable payloads and include both missing and extra identities. The final explanation should reject a sort-only implementation and make clear which policy changes would affect other callers of the shared helper.
