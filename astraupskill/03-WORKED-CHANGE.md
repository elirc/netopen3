# Build one lookup and consume requested matches

The replacement helper first enumerates hydrated entities and attempts to add each key and payload to a dictionary. A duplicate key leaves the original entry intact. No arbitrary last-write replacement or unstable sort tie determines which payload survives.

It then walks requestedIds in order. A successful Remove returns the stored entity and appends it to the result. A missing key does nothing. Because matched entries are removed, a repeated requested identifier cannot append the same entity twice. Any unrequested dictionary entries remain unused and never reach the response.

The template controller continues to call the shared helper before mapping. Its Total assignment and existing empty-page branch are unchanged. The new tests protect those surrounding behaviors while exposing the helper's old extra-entity and duplicate cases.

Review the algorithm with concrete sequences rather than only a normal sorted example. Requested A, missing, B with hydrated B, extra, A must yield A, B. Requested B, A, B with duplicate hydrated payloads must yield one B and one A using the first payload for each. The ordinary scrambled-order regression remains valuable, but those mismatched-set examples are what distinguish the repair from a simple sort refactor.

## Before and after

The new helper is printed in full in [02-CONCEPTS](02-CONCEPTS.md#source-excerpt) and lives at `Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs:90-110`. The line it replaced is preserved in [the snapshot](snapshots/Umbraco-CMS__src__Umbraco.Cms.Api.Management__Controllers__ManagementApiControllerBase.cs.original.txt) at lines 90-92:

```cs
    protected static List<TEntity> OrderByRequestedIds<TEntity>(IEnumerable<TEntity> entities, Guid[] requestedIds)
        where TEntity : IEntity
        => entities.OrderBy(e => Array.IndexOf(requestedIds, e.Key)).ToList();
```

Work the two mismatched examples through this one-liner by hand. `OrderBy` is a stable sort, so ties keep hydration order. For requested A, missing, B with hydrated B, extra, A the result is extra, A, B. For requested B, A, B, A with hydrated A-first, B-first, B-later, A-later it is B-first, B-later, A-first, A-later: four items where the contract wants two.

**Blast radius.** The helper sits in the shared base class, so this change reaches every caller, not only templates. A scoped search (`git grep -n "OrderByRequestedIds(" -- Umbraco-CMS/src/Umbraco.Cms.Api.Management`) finds ten call sites. Five are search-item controllers (`DataType`, `Element`, `MediaType`, `MemberType`, `Template`). Five are batch controllers (`BatchDataTypes`, `BatchDocumentTypes`, `BatchMediaTypes`, `BatchMemberTypes`, `BatchUsers`). Only the template controller received the new mismatched-set tests. The other four search controllers keep one ordering test each. The batch controllers are exercised only by `Umbraco.Tests.Integration/ManagementApi/*/Batch*ControllerTests.cs`, which the focused acceptance run did not execute.

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
