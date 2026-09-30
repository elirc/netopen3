# 04. Pagination metadata and sparse pages

The controller returns a [PagedModel](../../../Umbraco-CMS/src/Umbraco.Core/Models/PagedModel.cs) containing Items and Total. These fields answer different questions. Items describes the hydrated mapped survivors on the current page. Total comes from the search stage and describes its broader matching population. Read the controller and regression test that preserves a total of nineteen while returning only two surviving items.

## Count is not total

Suppose search returns page keys A, M, B with Total nineteen. Hydration returns A and B but not M. The helper omits M, and the response contains two items while retaining Total nineteen. Replacing Total with two would turn a page-local observation into a statement about the whole search population. The controller does not have evidence to make that rewrite.

Likewise, an empty search page can retain a nonzero total, for example when an offset lies beyond the current page range under the search result supplied by a test. The empty branch returns no mapped items but preserves the total. A response with no items does not necessarily mean no matching entities exist anywhere. Clients need to interpret page position and metadata together.

## Sparse hydration has a consistency meaning

Missing hydrated entities can arise in a designed fixture and may arise from changes between stages in a real system, but the narrow controller source does not prove a specific production cause. Do not automatically call it index lag, especially when this path uses direct database entity search. The useful contract is that reconciliation tolerates disagreement without including unrelated entities or corrupting metadata.

If a future product wants to refill sparse pages, it needs a new algorithm. Fetching more search identities changes how skip/take, total and duplicate suppression interact. A helper that simply orders the current hydrated batch cannot silently promise full page length. State the current behavior first, then design refill as an explicit extension with bounds.

## Total authority should be documented

The search service is the source of Total in this response. Hydration does not return a replacement total, and the mapper has no count authority. This ownership rule makes maintenance easier. A mapper refactor that filters output should not casually recompute Total; it should raise a contract question about what the new filtering means.

Use names in test fixtures that make the distinction obvious: searchTotal, pageKeys, hydratedTemplates and responseItems. A generic variable called count invites accidental substitution. The regression test's nonmatching values are intentional: if Total equaled returned length in every fixture, an incorrect rewrite could pass unnoticed.

## Exercise UM-04A: build a metadata matrix

Create cases with total zero and empty page, total seventeen and empty page, total nineteen with one missing hydrated key, duplicate requested identities with total four, and all hydrated entities missing for a nonempty search page. Record response Items and Total separately. Do not force them to match merely because a UI summary prefers one number.

For the all-missing nonempty case, distinguish it from the empty-search short circuit. The controller still calls hydration and then mapping on the empty selected sequence. Service-call behavior differs even when the final Items list is empty. Later testing chapters use this distinction to avoid overgeneralized short-circuit assertions.

## Exercise UM-04B: design a client message

Write user-facing text for a sparse page that does not claim the entire search is empty. Decide whether to show total matches, current returned count and navigation context. Keep the wording honest about the data available. Do not expose an implementation speculation such as search index is stale unless the system actually diagnosed that cause.

If the client chooses to refresh, describe how it avoids an infinite loop when the same sparse page persists. A refresh is an action, not proof that the underlying disagreement will disappear. An explicit empty-page state can be more useful than repeatedly loading without explanation.

## Exercise UM-04C: propose bounded refill

Design a future algorithm that attempts to fill a page after missing hydration. Define a maximum number of extra search batches, how requested order is preserved, how duplicates across batches are suppressed, and what cursor or offset the client uses next. Explain why simply increasing take on the hydration call is meaningless when hydration receives identities rather than a search window.

Include a case where every candidate is missing and one where duplicate identities span batches. The algorithm must terminate and return an honest partial result when its budget is exhausted. Preserve the authority of search Total unless the product deliberately changes what that field means.

## Review standard

A strong answer keeps search-wide metadata separate from current-page survivors, uses deliberately unequal counts in tests, and distinguishes empty search from empty hydration. A refill proposal should be bounded, preserve ordering and explain navigation semantics. The final report should avoid diagnosing a production consistency cause that the narrow source trace does not establish.
