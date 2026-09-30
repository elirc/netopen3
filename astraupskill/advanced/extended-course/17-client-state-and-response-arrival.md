# 17. Client state and response arrival

A server can return the correct ordered page while a client displays the wrong search result. The missing boundary is often request identity: two valid requests finish in a different order from the user's latest intent. This chapter develops a client-side learning model tied to the template-search response contract. It does not claim to describe an inspected back-office component; the client behavior here is an explicit exercise design.

The server action in [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs) returns a paged response containing mapped items and the search total. It does not attach a client interaction generation to that response. A proposed client can maintain such a generation locally so that older responses do not overwrite the state of a newer search.

## Distinguish request completion from current intent

Imagine a user types landing, then changes the query to news. Request R1 searches landing; request R2 searches news. R2 finishes first and displays news items. R1 then finishes and replaces the list with landing items while the input still says news. Every server response may be individually correct. The incorrect state arises because the client applies an outdated response to a newer interaction.

Assign a monotonically increasing local generation when the search intent changes. R1 captures generation seven and R2 captures generation eight. Before applying a response, compare its captured generation with the current generation. R1 is ignored after generation eight becomes current. The generation is a client state-management tool, not an entity version, a server transaction identifier, or an authorization token.

Cancellation can reduce unnecessary work, but it should not be the only stale-response defense. A cancellation request may arrive after a response has completed or may not interrupt every server dependency. The actual action accepts a CancellationToken, but its body does not forward that token to the synchronous search call or the Guid hydration overload. A client must not assume that requesting cancellation proves the older response cannot arrive.

## Model the state explicitly

Use a state record containing query, generation, items, total, loading status, and error. A search-start transition increments generation and captures the new query. A success transition applies only if its generation still matches. A failure transition follows the same guard so an old failure cannot erase a newer success. A cancellation completion also needs a rule; otherwise the UI can remain stuck in a loading state or clear the wrong request's indicator.

Decide whether a new search clears existing items immediately or keeps them visible while marking them as previous results. Both designs can be coherent. Keeping old items without a visible distinction may mislead the user into believing those items match the new query. Clearing immediately may cause unnecessary visual movement. The contract should state which query the displayed items represent while a new request is pending.

The total belongs to the same applied response as the items. Do not apply items from generation eight and total from generation seven through separate unguarded updates. The server already distinguishes total matching candidates from hydrated item count. Mixing generations adds another inconsistency. Apply the response fields together under the same generation check in the learning model.

## Sparse pages do not mean no matches

Suppose the response has total twelve and two items for a requested candidate page of five. The current action can produce a sparse result after missing hydration or duplicate selection. A client label such as two matches would conflate displayed items with the search total. A better design might say showing two available items from this result page while retaining the reported total according to the product contract.

Do not infer the next offset from the number of hydrated items. The server's search operates in candidate positions, and the controller preserves its total. Advancing skip by two after requesting five candidates risks re-examining part of the same candidate range. Under the existing aligned skip/take service contract, ordinary paging should use the defined candidate-page increment rather than the displayed count.

An empty visible page with a positive total requires careful wording. It can reflect a page beyond available results, missing hydration, or other differences established by the server contract. The client should not automatically rewrite the total to zero. It may offer refresh or navigation according to a defined policy, but it should avoid claiming that the entire search has no matches from one sparse page alone.

## Selection identity survives representation changes

For a template picker, the selected value should normally be an identity under the proposed UI contract, not a list index. If hydration or paging changes the list, index two can refer to a different template. A stable key allows the client to distinguish selection from display position. The current server helper is also identity-based, but that fact alone does not force a correct client implementation.

Consider a selected template whose name changes after a refresh. The UI can retain the key while updating the displayed label from the latest applied response, if the product permits that behavior. If the selected key disappears from the current page, disappearance does not necessarily mean the template was deleted; it may simply no longer belong to the current search. A selection policy must distinguish unavailable, unselected, and not currently displayed.

Keyboard focus is another separate state. Replacing a list should not silently move a user's intended action to a different entity merely because the same row index remains focused. A robust design can preserve focus by key when the key remains present, otherwise choose an explicit fallback. This is an accessibility and correctness concern in the proposed client exercise, not a claim that a particular upstream component mishandles focus.

## A deterministic disposable client lab

Build a small pure model with start, resolve, and reject functions. Use manually completed promises or explicit event objects so you can deliver responses in any order. No browser, network, or running Umbraco site is necessary to test the generation rule. The model should retain a log of transitions, making it clear why an old result was ignored.

Run a schedule where R1 starts, R2 starts, R2 succeeds, and R1 succeeds. Then replace R1's success with failure. In both cases the state should still represent R2. Run another schedule where R1 finishes before R2 starts to ensure legitimate earlier results are not rejected unnecessarily. A guard that discards every response can pass a badly designed stale-response test, so include a positive application case.

Add a sparse-page response with a total larger than the item count and verify that both values remain distinct. Add an item reorder and verify selection by key rather than index. These tests explore a client contract using the real response shape concept, but they do not prove the actual back-office client uses the model. Label the artifact as a proposed client-state exercise.

## Exercise UM-17A: trace response arrival

Write a transition table for two searches whose responses arrive in reverse order. Include query, generation, loading state, items, total, and error after each event. Show the incorrect state without a generation guard and the corrected state with one. Include an old failure arriving after a new success.

## Exercise UM-17B: design sparse-page presentation

Create display copy and navigation rules for total twelve, requested take five, and two returned items. Explain which number determines candidate-page advancement and which number is displayed as the available item count. Include an empty visible page with a positive total and avoid asserting a deletion that the response does not establish.

## Exercise UM-17C: separate selection and focus

A user selects B at row two. A refresh returns C, A, B instead of A, B, C. Define the selected value and keyboard-focus behavior. Then consider a refresh where B is absent. Provide acceptance tests for each transition and explain what additional evidence would be needed to apply the design to the real back-office component.

## Review checkpoint

A strong client design associates all response fields with one request generation, treats cancellation as an optimization rather than a stale-response proof, and keeps identity separate from position. It respects sparse server pages without silently redefining Total. Its evidence remains scoped to the model until an actual UI integration is inspected and tested.
