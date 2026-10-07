# Keep selection separate from pagination metadata

The old sort places extra first because its requested index is minus one, followed by A and B. The corrected response contains A and B only. Moving extra to the end would improve order but still return an unrequested entity, so it would not satisfy the selection contract.

The duplicate example yields B-first followed by A-first. TryAdd preserves each first hydrated payload, and Remove consumes the entry on its first requested occurrence. Later duplicate requests find no entry and emit nothing. Distinct hydrated identifiers can arrive in any order without changing requested output order.

The empty page retains Total seventeen and calls neither hydration nor mapping. Total describes the search result, not simply the count of hydrated page items. Missing entities can reduce the returned page without authorizing this helper to rewrite that metadata.

Reversing hydration with duplicate payloads can change the selected alias because the policy explicitly retains the first hydrated entity. The tests document that choice rather than claiming a canonical payload across contradictory service responses. In numbers, with *h* hydrated and *r* requested items: the old helper calls `Array.IndexOf` (O(r)) once per hydrated entity inside a sort, so roughly O(h·r + h log h). The new one builds the dictionary in O(h) and walks the requested ids in O(r), which is O(h + r) on average, plus O(h) extra memory for the dictionary.

## Break-it answers (derived by reading the tests, 2026-10-06)

| Edit in a disposable copy | Fails | Still passes |
|---|---|---|
| Restore the old `OrderBy(Array.IndexOf)` one-liner | the id assertion in `…Excludes_Unrequested_And_Missing…` (actual `extra, A, B`); the id **and** alias assertions in `…Collapses_Duplicate_Keys…` (actual 4 items) | the scrambled-order test (no extras or duplicates), the empty-page test (the helper is never reached) |
| `TryAdd` → indexer assignment | only the alias assertion in `…Collapses_Duplicate_Keys…` (actual `b-later, a-later`) | the id assertions, because `Remove` still emits each key once |
| Delete the empty-page `return` | `_templateService.Verify(… Times.Never)` in the empty-page test | `Items` is empty and `Total` is 17 either way, so only the call-count assertion detects the missing short circuit |

The last row is the useful lesson. The short circuit is an optimisation, and only a call-count assertion can see it.

## Review checklist

During review, check extras, missing keys, requested duplicates, hydrated duplicates, order and total independently. Then check the shared helper's other callers. [03](03-WORKED-CHANGE.md#before-and-after) lists ten: four other search controllers that keep only a single ordering test each, and five batch controllers covered only by integration tests that this change's acceptance run did not execute. A senior reviewer would ask for either one shared helper test that covers the mismatched sets once for all callers (the helper is `protected static`, so it needs a small derived test class), or an explicit statement that the batch endpoints were not re-verified. These unit results do not establish production authorization or synchronization between an index and persistent storage.

## Source excerpt

From [Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs](../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs).

```cs
    protected static List<TEntity> OrderByRequestedIds<TEntity>(IEnumerable<TEntity> entities, Guid[] requestedIds)
        where TEntity : IEntity
    {
        var entitiesById = new Dictionary<Guid, TEntity>();
        foreach (TEntity entity in entities)
        {
            // The first hydrated entity wins if a service returns duplicates.
            entitiesById.TryAdd(entity.Key, entity);
        }

        var ordered = new List<TEntity>();
        foreach (Guid requestedId in requestedIds)
        {
            if (entitiesById.Remove(requestedId, out TEntity? entity))
            {
                ordered.Add(entity);
            }
        }

        return ordered;
    }
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
