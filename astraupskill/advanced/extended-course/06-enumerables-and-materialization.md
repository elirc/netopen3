# 06. Enumerables and materialization

IEnumerable describes how values can be iterated; it does not say whether they already live in a list, are generated lazily, or trigger work when enumerated. The search controller uses Any, Select and ToArray on search items, and the ordering helper enumerates hydrated templates into a dictionary. Read [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs) and the concrete [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs).

## Inspect the concrete source before estimating effects

The local entity-search implementation materializes its results to an array before returning PagedModel. For that implementation, the controller's Any followed by Select/ToArray iterates an in-memory array rather than issuing the entity query twice. A mock or alternate implementation could return a different enumerable, so the interface alone does not establish the same behavior for every possible provider.

This is a useful distinction between a broad API type and a concrete path. Do not report a double database query simply because you see two LINQ operations. Conversely, do not assume every enumerable is harmless to enumerate repeatedly. Follow the actual provider and materialization boundary before making a performance claim.

## One-shot and stateful sequences are assumption probes

A synthetic enumerable can count enumerations, throw on a second traversal or yield different values each time. Such fixtures help expose hidden assumptions. The controller's initial Any and later key extraction would matter for a one-shot search sequence. The helper itself builds a lookup in one pass over hydration and then traverses the requested array.

These probes are not automatically production bugs. The concrete search implementation already returns an array. The lesson is to document what the controller expects from its service contract and decide whether materializing once at the boundary would improve robustness. A proposed change should consider allocations and test behavior, not merely follow a blanket rule that LINQ must be avoided.

## Materialization fixes a view, not all consistency

ToArray captures the current enumeration's keys into a stable sequence for hydration and ordering. It does not make the later template-service call part of the same database snapshot. Entities can be missing or differ between stages under some operational scenarios. The helper's omission policy handles the supplied disagreement, but the array itself is not a transaction boundary.

A useful diagram has two kinds of stability: the requested-key array no longer changes during the helper call, while underlying data may have its own lifetime and consistency rules. Avoid using snapshot loosely for both. A materialized collection is an in-memory snapshot of one enumeration; a database snapshot has a stronger and separately established meaning.

## Exercise UM-06A: count enumerations

Create a deterministic counting enumerable for slim search entities and another for hydrated templates in a disposable harness. Record each GetEnumerator and MoveNext call at a level appropriate to the experiment. Compare empty and nonempty search paths. Explain which sequence is skipped entirely on the empty branch.

Do not infer a network request from every MoveNext. Your wrapper measures iteration. If you want to measure database calls, instrument the real repository boundary separately. The report should name its observer so another learner does not overinterpret a useful small experiment.

## Exercise UM-06B: propose single materialization

Consider materializing search items once at the controller boundary before checking emptiness and extracting keys. State the behavior it would preserve, the assumptions it would strengthen, and the allocation it might add or reuse. Write a test with a stateful enumerable that demonstrates the proposed benefit while retaining ordinary array cases.

This is a design exercise, not an instruction to change the application immediately. The current concrete service returns an array, so the practical benefit may be limited. A good recommendation can be to document the service contract rather than add defensive work without a demonstrated caller need.

## Exercise UM-06C: examine deferred mapping

The mapper interface returns a list for MapEnumerable, while the helper returns a list even though the controller variable is typed as IEnumerable. Identify where output becomes materialized and which exceptions can occur during each stage. If a mapping callback throws, the controller does not have a completed response list merely because hydration succeeded.

Compare this with a proposed lazy response enumerable. Serialization could then trigger mapping later, changing where failures occur and which resources must remain available. A lazy refactor can alter error timing even when successful values look identical. Include failure timing in the review, not only output equality.

## Review standard

A strong answer connects interface types with actual implementations, measures iteration without inventing database effects, and distinguishes in-memory materialization from transactional consistency. It explains why deferred execution can change failure timing and why a proposed optimization should preserve both values and resource ownership.
