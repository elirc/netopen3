# 22. Capstone: a continuation contract across server and client

Design a coherent search experience when candidate pages are sparse and the dataset can change. The capstone combines a proposed server continuation model with a client request-generation model. It asks for a precise contract and a disposable prototype, not an immediate modification of Umbraco's existing endpoint. The current action remains a one-search, one-hydration reconciliation path with skip and take parameters.

Use [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs), [PaginationHelper](../../../Umbraco-CMS/src/Umbraco.Core/PaginationHelper.cs), and [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs) as the source constraints. The service converts aligned offsets to page numbers and requests text ordering. The controller preserves the reported total while omitting unavailable hydrated entities. Any proposed continuation feature must explain how it relates to those existing behaviors.

## Choose the product promise first

Select one of two goals. The first is an interactive picker that tolerates changing results but must not display an old query response over a newer query. The second is a stable export that requires a well-defined result set. These goals need different guarantees. Do not claim to satisfy both with a generic cursor unless you explain how the stronger snapshot requirement is established.

For the picker route, a bounded candidate traversal with explicit sparse-page behavior may be enough. For the export route, you may need a stored result set, a storage snapshot, or another stable membership mechanism. The prototype can model that mechanism, but the report must identify what the real query and storage layers would need to implement before the guarantee becomes true.

Write an example before drawing the architecture. A user requests four visible items. Candidate batches contain missing entities and duplicate identities. Meanwhile, the user changes the query and an earlier request finishes late. Your contract must define both which candidates the server has consumed and which response the client may apply. Solving only one side leaves a plausible wrong result.

## Server continuation state

Separate candidate progress from emitted item count. A proposed token or server-side record can contain the last examined candidate position, query identity, ordering version, and any buffered remainder required by the chosen page-size promise. If a batch produces more available entities than the requested visible size, explain what happens to the unused ones. Discarding them silently is not a complete continuation design.

Choose a finite work budget. Define maximum batches or candidates per request and a clear outcome when the budget is exhausted before the visible target is met. An empty visible result after a finite scan is not proof that no matches exist. The response contract should make partial progress understandable, whether through explicit continuation metadata or a documented sparse-page model.

If you choose to keep the existing response shape, acknowledge the resulting limits. Some continuation outcomes may not be distinguishable to the client without additional fields or a new endpoint version. Compatibility is a real constraint, not a reason to hide state transitions. A design can legitimately recommend retaining ordinary sparse paging instead of refill if the added complexity does not serve the product.

## Ordering and mutation

A proposed cursor needs a total order, including a unique tie-breaker for equal names. The database comparison and cursor predicate must agree. The inspected service call requests text ordering, but this workbook has not established every lower-layer tie rule or collation. Treat those as integration requirements rather than filling them in with a convenient in-memory comparer.

Model insertion before a boundary, deletion before a boundary, and rename across a boundary. An offset and a tuple cursor react differently. A cursor can avoid some offset shifts while still allowing a renamed entity to move across the traversal boundary. If the product requires a fixed export, identify a mechanism that prevents those changes from altering the chosen result set.

Duplicate candidate identities across batches also need a rule. Track successfully emitted identities if a later available payload should be allowed after an earlier hydration miss. Track all seen candidates if the contract instead consumes each candidate identity once regardless of hydration outcome. Either decision affects what a client can expect; neither should emerge accidentally from a poorly named set.

## Client generation and selection

The client model maintains a generation for each new search intent. A response can update items, total, continuation, loading state, and error only when its captured generation matches the current one. Apply those fields together. A stale response that updates only the continuation token can be just as damaging as one that replaces the visible items, because the next-page request may then continue the wrong search.

Cancellation can be requested when the query changes, but generation checks remain necessary. The current action's token parameter does not by itself prove every downstream operation can be interrupted. A late completion must have a deterministic disposition in the client model. Ignoring a stale response is a state transition decision, not evidence that the server stopped doing work.

Keep selection by identity rather than row position. If a refreshed page reorders items, the selected key should not silently change to whichever entity occupies the old index. Define how an out-of-view selection is displayed and how keyboard focus moves. The prototype need not implement a complete accessible UI, but its state contract should not force an inaccessible or misleading integration later.

## Required adversarial schedules

Include a schedule where request one starts, request two starts, request two succeeds, and request one succeeds afterward. Include a variant where request one fails afterward. In both cases the current query's state should remain intact. Include a positive schedule where the first response completes before the second request begins so a model that discards every response cannot pass accidentally.

On the server side, include permanent hydration sparsity, repeated candidate keys across batches, an extra hydrated entity, a partial final batch, and a downstream failure after some progress. Each schedule should terminate within the declared budget. Record whether the outcome is complete, sparse, budget-limited, cancelled, or failed according to your proposal. Avoid using a generic success flag for meaningfully different states.

Add a query-mismatched continuation case. A token issued for landing should not silently resume news unless the contract explicitly defines that transformation. Add an expired token case and an altered token case at the model boundary. A pure model can test rejection decisions, but cryptographic integrity and real authorization binding require an actual mechanism and its own tests.

## Prototype boundaries and evidence

Use synthetic identities and a small in-memory candidate source to control every schedule. Label that source as a model. It does not reproduce database collation, query translation, transaction isolation, or live authorization. The value is deterministic exploration of continuation and client state, not a claim that the production search service has gained cursor support.

Separate model tests from proposed integration tests. A model can prove that your loop consumes a finite batch budget under permanent sparsity. A database integration test is needed to establish that a proposed resume predicate returns the correct next tuple under the configured ordering. A hosted test is needed to establish endpoint binding and policy behavior. A browser-level test can add evidence about actual focus and rendering.

The report should include an evidence matrix with claim, artifact, executed status, and limitation. This prevents a successful pure model from being cited later as proof of a deployed feature. It also gives a future implementer a clear sequence of remaining tasks instead of a vague instruction to add integration tests.

## Migration and compatibility

Describe whether the feature extends an existing response, introduces a versioned endpoint, or remains an internal client adaptation over current sparse pages. Explain the old client's behavior when new fields are present and the new client's behavior when they are absent. Do not assume every consumer is upgraded simultaneously.

Plan how expired or invalid continuation state is surfaced. Silently restarting from page one may create duplicates and confuse users. A documented restart requirement can be clearer, especially when the query or caller context changed. If server-side state is used, define its lifetime and cleanup policy so a small navigation feature does not accumulate unbounded state.

Include a rollback story. If the new continuation implementation is disabled, what happens to tokens already issued? A clear unsupported-or-restart outcome is safer than interpreting them under a different ordering version. Compatibility includes transitions during rollout and rollback, not only the steady-state happy path.

## Exercise UM-22A: specify and prototype one promise

Choose picker or stable export. Write the guarantee, its non-guarantees, and a state diagram. Implement a disposable model with finite work limits and explicit continuation. Include at least one schedule where candidate progress differs from emitted count. Explain which real storage capability is still missing from the model.

## Exercise UM-22B: join server and client schedules

Connect the model response to a client generation state machine. Deliver responses out of order, including a stale failure and a stale continuation token. Verify that all fields remain associated with the current query. Add selection-by-key behavior across a reorder and state what happens when the selected key is absent from the current page.

## Exercise UM-22C: defend compatibility and rollout

Prepare a review note covering response evolution, token validation, expiration, ordering version, work budgets, and rollback. Identify one rejected simpler alternative and explain the product requirement that justifies the added complexity. If no requirement justifies it, recommend retaining current sparse paging and document how the client can handle it clearly.

## Completion rubric

The capstone passes when continuation, ordering, work limits, and client generation form one coherent contract. Every adversarial schedule must have a defined result. The strongest submission recognizes when a modest sparse-page design is enough and reserves stronger snapshot claims for mechanisms that can actually establish them. A token-shaped string alone is not a completed feature.
