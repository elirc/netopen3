# 15. Bounded refill as an explicit proposal

The current template search action does not refill a page after hydration removes missing entities. It searches once, hydrates those keys, selects available entities, and returns the search total. This chapter proposes a different feature for study: scan additional candidate pages until enough unique visible items have been collected or a strict budget is exhausted. The proposal is deliberately separate from the inspected implementation.

Revisit [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs) and [PaginationHelper](../../../Umbraco-CMS/src/Umbraco.Core/PaginationHelper.cs). The search service receives skip and take, and the helper converts aligned offsets into page numbers. Any refill design using that service must respect its paging contract. Arbitrarily advancing skip by the number of returned hydrated records would confuse candidate positions with emitted positions and can violate alignment.

## Define the unit of progress

Suppose the client requests five displayed items. The first candidate page contains A, B, C, D, E, but only A and D hydrate. The current action returns two items. A refill proposal could fetch the next candidate page, F, G, H, I, J. If F, H, and J hydrate, the proposed response now has five items. It has examined ten candidate positions and emitted five entities.

The next request cannot safely start at candidate offset five merely because five items were displayed. It would rescan the second candidate page. Nor should it start at offset ten without considering unused hydrated candidates if the second page yielded more than needed. A response contract must specify whether those extra candidates are retained, skipped, or represented in continuation state. Page fullness is not just a local while-loop concern.

Use separate variables for requested output size, candidate page size, candidates examined, unique items emitted, and continuation position. Names matter here because a single variable called count invites accidental substitution. An emitted item count is not a search offset. A total matching count is not a remaining work budget. A candidate batch size is not necessarily the client-visible page size.

## Put finite limits before the loop

An unbounded refill loop can turn a small page request into a large amount of work when hydration is sparse. Define a maximum number of candidate pages, a maximum number of candidate identities, and a time or cancellation policy appropriate to the underlying APIs. Only claim enforceable limits. If a downstream synchronous call cannot observe cancellation, checking a token between calls does not interrupt that call halfway through.

For a pure learning model, choose candidate page size five and a maximum of three pages. With no hydratable entities, the model must stop after fifteen candidates at most. It returns an empty visible page with a continuation state and a reason such as budget exhausted, if that is the proposed response contract. It must not claim that the dataset contains no matches merely because the hydration budget was exhausted.

A budget also needs a compatibility story. The existing response model may not expose a reason or continuation field. Adding fields, introducing a new endpoint, or using a versioned response are product and API design choices. The chapter does not imply that a new enum can simply be added without considering existing clients. Start with a design worksheet, then implement a disposable model before changing application code.

## Preserve order across batches

Each candidate batch has its own ordered identity sequence. The proposed overall result should follow the concatenated candidate order under the chosen contract. Deduplicate across the entire request, not merely within each batch, if the intended visible result contains each identity once. The existing helper's dictionary removal handles duplicates within one selection call; calling it independently on two pages does not automatically prevent the same key from appearing in both outputs.

Consider first-page keys A, B, C and second-page keys C, D, E. A request-scoped emitted-key set prevents C from being returned twice if both appearances hydrate. However, decide what happens when C failed to hydrate on the first page and succeeds on the second. If the set tracks candidates seen rather than items emitted, the later available C may be suppressed. Those two policies answer different questions. Write the intended rule before choosing the set name.

The payload duplicate policy also remains relevant. Within one hydration response, the current helper selects the first payload for a key. Across multiple hydration calls, a refill design could observe changed payloads. Once an item has been emitted into the response candidate list, replacing it with a later payload would add another freshness rule. The simplest model may retain the first successfully selected payload, but that must be stated as a proposal rather than attributed to the current one-shot action.

## Worked budget trace

Take an output target of four, batch size three, and a limit of three batches. Batch one requests A, B, C; only B is available. Batch two requests C, D, E; C and E are available. Batch three requests F, G, H; all three are available. A proposed first-successful-payload policy collects B, C, E, F and has two remaining hydrated candidates, G and H.

At this point, the difficult part is not selecting F. It is defining the continuation. If the server advances beyond H and discards G and H, later pages may never show those valid entities. If it starts the next request at F, duplicates appear unless continuation remembers partial batch consumption. If it buffers G and H server-side, that buffer needs lifetime, ownership, and invalidation rules. If it encodes remaining candidate positions in a signed continuation token, it needs a stable way to resume the underlying query.

One intentionally simpler alternative is to avoid promising exactly full visible pages. Return every selected item from a bounded number of whole candidate batches and document a maximum response size based on that budget. Another is to keep current sparse paging and improve client explanation. Both may be more appropriate than refill. The exercise should evaluate alternatives, not assume the feature is desirable because it is possible to write a loop.

## Failure and cancellation are part of the proposal

Suppose batch one succeeds and batch two hydration fails. Does the endpoint return a partial success, fail the whole request, or return a typed incomplete result? Each choice affects retries and client interpretation. Returning the first batch with an ordinary complete-success envelope may conceal a failure. Failing the request may repeat work, but it avoids presenting an unexplained truncation as a complete page.

If the operation is read-only, repeating reads does not itself create a write-side duplicate, but the returned membership can change between attempts. That still matters for navigation. A retry policy must not promise that the same visible page is reproduced unless the underlying continuation or snapshot mechanism establishes it. Read-only does not mean semantically repeatable.

For cancellation, define observation points before each new candidate fetch and before starting hydration. Record any unavoidable interval inside synchronous dependencies. A test can cancel at a controlled boundary and assert that no later batch starts. It cannot infer interruption of a running database command from a token check in an outer loop. The proposal should preserve this distinction in both documentation and test names.

## Exercise UM-15A: calculate the continuation problem

Use the three-batch trace above. Produce a ledger for candidate positions, hydration successes, emitted items, and leftover items. Compare discarding leftovers, buffering them, and returning a larger bounded page. Explain which option risks omission, which introduces state, and which changes the client-visible page-size contract. Recommend one option for a clearly stated product need.

## Exercise UM-15B: specify a bounded algorithm

Write pseudocode for a disposable model with a maximum candidate count and maximum batch count. Define whether duplicate tracking records seen candidates or successfully emitted identities. Include empty batches, missing hydration, duplicate candidates, and a downstream failure. State a termination argument that does not depend on eventually finding a valid entity.

## Exercise UM-15C: challenge the feature request

A stakeholder asks for always-full pages without changing the existing response shape or service contract. Explain why the request hides requirements about continuation, sparse data, and failure. Offer two achievable alternatives with acceptance examples. The goal is a precise negotiation of behavior, not an automatic rejection of the request or an unbounded implementation.

## Review checkpoint

A strong proposal distinguishes candidate positions from emitted positions, terminates under permanent sparsity, and makes continuation semantics explicit. It labels every new behavior as a proposal. It does not claim that the existing action refills, that the current helper deduplicates across requests, or that an outer cancellation check interrupts synchronous storage work.
