# Build one lookup and consume requested matches

The replacement helper first enumerates hydrated entities and attempts to add each key and payload to a dictionary. A duplicate key leaves the original entry intact. No arbitrary last-write replacement or unstable sort tie determines which payload survives.

It then walks requestedIds in order. A successful Remove returns the stored entity and appends it to the result. A missing key does nothing. Because matched entries are removed, a repeated requested identifier cannot append the same entity twice. Any unrequested dictionary entries remain unused and never reach the response.

The template controller continues to call the shared helper before mapping. Its Total assignment and existing empty-page branch are unchanged. The new tests protect those surrounding behaviors while exposing the helper's old extra-entity and duplicate cases.

Review the algorithm with concrete sequences rather than only a normal sorted example. Requested A, missing, B with hydrated B, extra, A must yield A, B. Requested B, A, B with duplicate hydrated payloads must yield one B and one A using the first payload for each. The ordinary scrambled-order regression remains valuable, but those mismatched-set examples are what distinguish the repair from a simple sort refactor.

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
