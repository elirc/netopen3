# Ordering cannot admit an unrequested entity

Sorting by a requested identifier's array position assumes every hydrated entity occurs in that array. Array.IndexOf returns minus one when it does not. Sorting ascending then places that extra entity before all requested matches instead of excluding it. The defect is selection as well as ordering.

The corrected contract starts from the requested identifiers as the authority for the output set. Hydrated data supplies payloads for those identifiers. Missing payloads are skipped, extra payloads are excluded, and duplicates have an explicit policy: first hydrated payload and first requested position win.

A dictionary supports average linear work over the hydrated and requested sequences. TryAdd retains the first payload. Remove during requested iteration both retrieves the match and prevents a repeated requested identifier from emitting it again. Output order comes from the requested sequence, not dictionary enumeration order.

Search Total is independent metadata. A page can contain fewer hydrated items because records disappeared or hydration omitted them, while Total still describes the search result. An empty page beyond the available range can likewise retain a nonzero total. This helper does not synchronize the search index with storage, and selecting requested identifiers is not a substitute for the endpoint's authorization rules.

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
