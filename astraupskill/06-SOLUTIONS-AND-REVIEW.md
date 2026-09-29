# Keep selection separate from pagination metadata

The old sort places extra first because its requested index is minus one, followed by A and B. The corrected response contains A and B only. Moving extra to the end would improve order but still return an unrequested entity, so it would not satisfy the selection contract.

The duplicate example yields B-first followed by A-first. TryAdd preserves each first hydrated payload, and Remove consumes the entry on its first requested occurrence. Later duplicate requests find no entry and emit nothing. Distinct hydrated identifiers can arrive in any order without changing requested output order.

The empty page retains Total seventeen and calls neither hydration nor mapping. Total describes the search result, not simply the count of hydrated page items. Missing entities can reduce the returned page without authorizing this helper to rewrite that metadata.

Reversing hydration with duplicate payloads can change the selected alias because the policy explicitly retains the first hydrated entity. The tests document that choice rather than claiming a canonical payload across contradictory service responses. Average dictionary operations give linear passes at the cost of a lookup proportional to hydrated distinct keys.

During review, check extras, missing keys, requested duplicates, hydrated duplicates, order, and total independently. Also check shared-helper callers for compatibility. These unit results do not establish production authorization or synchronization between an index and persistent storage.

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
