# 14. Consistency between search and hydration

Search and hydration are separate observations. Reordering hydrated entities repairs sequence alignment, but it does not turn those observations into one immutable snapshot. This chapter uses the actual controller path to reason about missing records, renamed payloads, and changing page boundaries. The objective is to name a consistency contract accurately enough that a test can distinguish its guarantees from accidental behavior.

Read [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs), the concrete [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs), and the Guid overload in [TemplateService](../../../Umbraco-CMS/src/Umbraco.Core/Services/TemplateService.cs). Search materializes slim entities from a paged entity-service query. The controller extracts keys and awaits a separate template-service call. The selected full templates are then mapped. The controller body does not wrap these stages in an explicitly declared shared snapshot transaction.

## An ordering guarantee is not a freshness guarantee

Take a search page with keys A, B, C and total nine. At the time of search, the slim records have names Alpha, Bravo, and Charlie. Before hydration, imagine B is removed and C is renamed to Aaron. Hydration returns full C and A records. The helper produces A followed by C, because the search key sequence remains the ordering authority. If the mapper exposes the new name, the displayed names can be Alpha then Aaron.

This example is a controlled scenario for reasoning, not a claim that an executed database race has been reproduced. It shows why preserving search order does not necessarily imply that response names appear sorted under their later values. The action does not sort hydrated payloads again. Sorting them would create a different contract and could conflict with the order established by the search observation.

The total remains nine under the current controller even though only two items are returned. Missing hydration is omitted. The helper does not fill the gap, reconstruct the old payload, or decrement the total. A client that equates returned count with matching total would misread the response. A maintainer who silently changes total to two would erase the distinction instead of solving the consistency question.

## Separate identity, payload, and membership time

The search page answers a membership-and-order question using its own observation. Hydration answers a payload question using another observation. Mapping determines the response representation of those payloads. These stages can be internally correct and still describe different moments. Label the moments S, H, and M on a timeline so the distinction remains visible during review.

There is no need to invent a distributed search index here. The inspected EntitySearchService uses the entity service and database query abstraction, not a demonstrated asynchronous external search engine. The lesson concerns multiple reads and service boundaries. Two database reads can differ under concurrent changes even when they use the same physical database, depending on transaction and isolation behavior. The controller source alone does not establish those deeper details.

Likewise, Task.FromResult in the template service does not prove that the overall sequence is atomic. The Guid overload obtains repository results inside its scope and returns a completed task containing the enumerable. A completed task can shorten one scheduling path, but it is not a transaction shared with the prior search call. Atomicity must come from a stated storage or synchronization boundary, not from the absence of an asynchronous suspension in one method.

## Worked state ledger

Build a ledger with one row per observation. At S, the membership sequence is A, B, C and the total is nine. At H, the available payload dictionary contains A and C. During selection, A is removed from the dictionary and emitted, B has no entry, and C is removed and emitted. At M, the mapper receives exactly those two entities in that order. No row requires a guessed database isolation level.

Now change the scenario so hydration contains two C objects with different names. The first hydrated C wins because the helper uses TryAdd. This duplicate policy does not mean the winning object is newer. It means service enumeration order decides among duplicate payloads for one key. If freshness is required, a version-aware rule must be designed with trustworthy version evidence; a later object in an enumerable is not inherently a later revision.

Finally, imagine search repeats A. The helper emits it once. The ledger can therefore have fewer emitted items for two distinct reasons: missing payloads and repeated requested identities. Diagnostics that label every reduction as deletion would be inaccurate. A good investigation classifies each cause separately before drawing conclusions about storage consistency.

## Possible contracts and their costs

One reasonable contract is best-effort reconciliation: preserve search identity order, omit unavailable payloads, keep the search total, and allow payload freshness to reflect hydration. That description is close to the observed action behavior, although a public API specification would still need formal ownership and compatibility review. It is modest but understandable.

A stronger proposed contract might require one storage snapshot for both membership and payload. Such a design needs proof that both services can participate in the same transaction or snapshot mechanism. It may require changes below the controller, and its resource lifetime matters. Holding database state while mapping or waiting for unrelated work can introduce cost. Merely placing two method calls inside a lexical block does not establish the guarantee.

Another proposal could return revision evidence so a client knows which observations were used. That might mean a result-set token, an entity version, or a documented query timestamp with carefully limited meaning. A timestamp alone does not identify a database snapshot. A token is useful only if the server can interpret and enforce its semantics on later requests. Metadata should explain a real guarantee rather than decorate an uncertain result.

## Test schedules instead of sleeps

A direct controller fixture can control the data returned by search and hydration without relying on real concurrent writes. Configure search with A, B, C and hydration with A and C. Assert selected order, missing omission, and the original total. This establishes reconciliation behavior. It does not prove that a deletion race occurs in a production database, but it precisely tests the action's response to the resulting mismatch.

To test a real consistency mechanism, use a storage-aware integration fixture with explicit coordination points. One actor completes membership selection, another performs a defined mutation, and the first continues hydration under the proposed isolation policy. A barrier or controlled service wrapper is easier to interpret than arbitrary delay. Record transaction configuration and the expected visibility rule; otherwise the test can pass for timing reasons unrelated to the intended guarantee.

Avoid requiring one universal outcome before the contract is chosen. Under a best-effort contract, missing B may be acceptable. Under a snapshot contract, returning an earlier B payload may be required. Under a retrying contract, the request may be restarted with a new membership page. The same observed difference can be correct or incorrect depending on the promised behavior.

## Exercise UM-14A: classify a sparse response

Given requested keys A, B, A, C and hydrated payloads C-new, A-first, A-second, X-extra, write the full selection ledger. The total is twenty. Explain each omitted occurrence and distinguish duplicate request, duplicate payload, missing hydration, and unrequested entity. State which returned payload version is selected and why that is not evidence of latest-version selection.

## Exercise UM-14B: design a consistency experiment

Specify a deterministic test schedule for deletion between search and hydration. Provide one direct controller test and one proposed storage-aware test. Explain what the first proves immediately and what additional setup the second requires. Do not use sleep as the synchronization contract. Include the total and mapping assertions, not only the final item count.

## Exercise UM-14C: write a product-facing guarantee

Choose best-effort reconciliation or a proposed snapshot-backed design. Write a short guarantee that explains membership, payload freshness, missing entities, and total. Then list a concrete non-guarantee and an acceptance test. If you choose the stronger design, identify the lower-layer capability that must exist before the guarantee can honestly be published.

## Review checkpoint

The essential distinction is between preserving the order of known identities and preserving a historical world state. A strong answer does not use the word consistent without specifying which observations agree, at what boundary, and under what evidence. It can explain a sparse response without inventing an external index or assigning freshness semantics to enumeration position.
