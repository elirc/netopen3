# Review guide: HTTP evidence, consistency, continuation, and client state

Attempt chapters 13-17 before reading these worked answers. Each answer separates an observation of the current source from a proposed design. Several exercises deliberately have more than one defensible solution. The review standard is whether the contract is explicit, the evidence supports it, and the failure cases are handled without claiming features that have not been implemented.

## Hints before the worked answers

For chapter 13, draw the inheritance arrows before collecting policy names. For chapter 14, give search and hydration different time labels even if both calls finish quickly. For chapter 15, write candidate position and emitted count in separate columns. For chapter 16, include two records with the same name. For chapter 17, deliver responses in the opposite order from requests. These small changes make assumptions observable without needing a large dataset.

If an answer becomes vague, reduce the example to three or four synthetic identities and write every state transition. Do not replace a missing source investigation with a familiar framework convention. A named policy, a completed task, a cursor token, and an HTTP client each require context before they establish a guarantee.

## UM-13A: reconstruct the authority chain

The chain is SearchTemplateItemController, then TemplateItemControllerBase, then ManagementApiControllerBase, then the framework Controller base. The item base supplies versioned back-office route metadata for item/template and an API explorer group. ManagementApiControllerBase declares BackOfficeAccess and UmbracoFeatureEnabled policies together with other API and filter metadata. The search action supplies its GET search action route and API-version mapping.

TemplateControllerBase is a sibling branch beneath ManagementApiControllerBase. Its TreeAccessTemplates attribute is real, but the search controller does not inherit through it. A diagram that places that class between the item base and the management base is incorrect. A diagram that removes all authorization from the search endpoint is also incorrect because it ignores the management base declarations.

One additional mechanism to investigate is endpoint or application-model convention configuration. Another is a global authorization filter or fallback policy. Naming such a mechanism is not evidence that it adds a particular restriction here. The submission should identify the registration and the resulting endpoint metadata if it wants to extend the conclusion beyond the inspected class chain.

Full credit requires the distinction between declared metadata and effective enforcement. Partial credit is appropriate for the correct chain with an unqualified HTTP status claim. No credit should be given to an answer that imports TreeAccessTemplates merely because the folder name contains Template. The lesson is transferable: directory proximity helps find code but does not create runtime inheritance.

## UM-13B: two tests with different claims

The direct test can configure search keys B, A, C, hydrate only A and B in a different order, and assert mapped output B, A with the original total. It should verify that the template object type and distinctive query/paging arguments reach search. No principal is required for this isolated action invocation, and its absence must not be described as an authorization bypass observed over HTTP.

The hosted test should construct a host with the intended authorization pipeline and a controlled authentication mechanism. A rejected principal should receive the configured rejection outcome and should not cause the search collaborator to execute. If the test replaces authorization with an always-allow handler, it no longer tests rejection by the real policies. The exact challenge or forbid response requires the actual host configuration rather than a guessed convention.

Neither test proves the behavior of every deployed installation. Deployment can add configuration, reverse-proxy behavior, authentication settings, and other environment differences. The hosted fixture's value is that its configuration is explicit and reproducible. The direct fixture's value is that orchestration failures are easy to localize. Combining their evidence is stronger than relabeling either one as a complete security audit.

The rubric rewards independent failure signals: an incorrectly ordered admitted response should fail the direct test, while a rejected request reaching search should fail the hosted test. A single test that only checks for a nonempty response cannot distinguish either problem reliably.

## UM-13C: visibility before paging or after it

Filtering before paging can make the candidate total and page boundaries describe only visible entities, provided the query layer enforces the same visibility rule for both count and results. It may require a permission-aware query or service capability. Filtering after hydration is easier to express at the orchestration layer but can create sparse pages and leave Total describing a broader candidate set. Filtering during mapping is especially easy to obscure because mapping is usually expected to transform representations, not redefine authorization membership.

The recommended contract depends on product policy. If even the existence of hidden matches is restricted, preserving an unrestricted total may be inappropriate. If the caller is permitted to know the aggregate count but not every payload, a different contract may be acceptable. The answer must state the policy before declaring one location secure.

A defensible recommendation is to make visibility part of the authoritative candidate query when the product requires visible-only totals and ordinary paging. That is a proposed extension requiring implementation evidence. It is not a claim that the current search action already filters by individual caller permissions. The review should reject answers that infer unauthorized exposure from sparse pages alone without establishing which entities the caller is entitled to see.

## UM-14A: classify every omitted occurrence

Requested keys are A, B, A, C. Hydrated payloads are C-new, A-first, A-second, and X-extra. Dictionary construction retains C-new, A-first, and X-extra. The second A payload is ignored because TryAdd does not replace the first value. During requested-key iteration, the first A removes and emits A-first. B is missing. The second A finds no dictionary entry because the first occurrence consumed it. C removes and emits C-new.

The final selected sequence is A-first followed by C-new, and the controller's total remains twenty. X-extra is never requested and remains unselected. Four different reasons explain reductions or omissions: duplicate hydrated A, missing B, repeated requested A after consumption, and unrequested X. A diagnostic that reports four deleted templates would be wrong.

The suffix new is a fixture label, not a version guarantee established by the helper. C wins because it is the available C payload. A-first wins because it appeared first among hydrated A objects. The helper does not compare timestamps, versions, or names. An answer that says latest wins has introduced a rule absent from the inspected implementation.

Review both identities and payload references. If a test only asserts keys A and C, it can miss replacement of A-first with A-second. A payload marker or reference assertion makes the duplicate policy independently observable.

## UM-14B: a deterministic consistency experiment

For the direct test, configure search with A, B, C and a total of nine, then configure hydration with C and A. Assert the mapper receives A and C in that order and the response retains nine. This establishes the controller's response to missing hydration. It does not establish the database event that produced the mismatch.

For the proposed storage-aware experiment, use an explicit coordination point after candidate membership is selected. Allow a second actor to remove B and commit under a defined transaction setup. Then release hydration. The expected outcome must follow the selected consistency contract and isolation configuration. Under a snapshot-backed proposal, the first actor may be required to retain a historical view; under best-effort reconciliation, omission can be acceptable.

Do not use a delay as proof that deletion happened between the two reads. The test should observe completion of the mutation and the point at which the first actor continues. Its report should include storage provider, transaction scope, and visibility expectations. A passing schedule under one isolation setting is not proof of all settings.

The answer should also assert mapping and metadata. Returning two items in arbitrary order with a rewritten total would not preserve the current reconciliation behavior even though the count looks plausible. A complete test protects the whole intended boundary.

## UM-14C: a guarantee with a stated limit

A suitable best-effort statement is: the response preserves the order of matching identities selected by search, includes the first available hydrated payload for each requested identity, omits unavailable entities, and reports the search total. Payloads may reflect a later observation than membership. This closely describes the inspected orchestration while avoiding a promise of one historical snapshot.

A concrete non-guarantee is that displayed names are not necessarily sorted according to their later hydrated values after concurrent renames. An acceptance test can search A then B, hydrate B with a new name that sorts before A, and still expect A then B. The test demonstrates the ordering authority instead of sorting the response to conceal the difference.

For a stronger snapshot proposal, the answer must identify a mechanism that can supply membership and payload under one stable view. It might require shared storage transaction support or a versioned result set. Merely adding a timestamp or awaiting both calls does not establish it. The rubric rewards a smaller honest guarantee over a stronger phrase with no implementation path.

## UM-15A: leftovers determine continuation

The three batches examine A through H. B is emitted from the first batch; C and E from the second; F fills the fourth output slot from the third. G and H are available but unused. Advancing continuation after H and forgetting them omits valid candidates from later pages. Restarting at F repeats work and requires a way to avoid returning F again. Neither problem is solved by counting four emitted items.

Buffering G and H can preserve them, but introduces state with ownership, lifetime, expiration, and consistency questions. Returning all selected items from the final batch avoids those leftovers but changes the maximum visible page size. Retaining sparse ordinary pages avoids refill complexity altogether. The preferred option depends on whether exact page size is actually a product requirement.

For a small picker, a defensible recommendation is to retain sparse pages and offer a clear refresh or next-page action. For a workflow that requires a fixed number of choices per response, a continuation design may be justified, but its cost should be explicit. The answer earns credit for exposing the tradeoff, not for always choosing the most elaborate mechanism.

## UM-15B: termination must survive permanent failure to hydrate

A bounded model initializes candidate position, batch count, examined count, and a collection of emitted identities. Before every batch it checks whether either limit has been reached. Each fetched candidate batch advances the candidate position according to the underlying paging contract, regardless of how many entities hydrate. Successful selections are appended only under the stated duplicate policy. The loop stops on target completion, end of candidates, budget exhaustion, cancellation, or the chosen failure outcome.

The termination argument uses finite budgets rather than eventual success. With a maximum of three batches, a dataset where no candidate ever hydrates still causes no more than three batch attempts. If the model permits an empty batch with an unchanged continuation token, it must detect lack of progress or consume a budget; otherwise an empty response can create an infinite loop.

Tracking emitted identities allows a later successful hydration of an earlier missing key. Tracking all seen candidates suppresses that later opportunity. Either can be specified, but the implementation and test must agree. A failure in a later batch must follow an explicit whole-request or partial-result contract. Silent partial success is not a neutral default.

## UM-15C: negotiate the hidden requirements

Always-full pages cannot be guaranteed when fewer than the requested number of valid entities exist. Even when enough exist somewhere, finding them may require unbounded work without a budget. The current service contract also ties offsets to candidate pages, so continuing after a partially consumed batch requires additional semantics. These are requirements, not objections to making the interface useful.

One achievable alternative is bounded sparse paging: inspect one candidate page, return available entities, preserve the documented total, and let the client request the next aligned candidate page. Another is bounded refill with an explicit continuation token and incomplete-result indication when the work budget is exhausted. Each alternative has acceptance examples that describe empty and permanently sparse data.

The response should explain why keeping the response shape unchanged may prevent the client from distinguishing budget exhaustion from completion. It should not promise a hidden server buffer without discussing expiration and ownership. The rubric favors a contract that a user can understand and a test can reject when violated.

## UM-16A: equal names need evidence

Use A and B both named Landing and a page size of one. If one query orders A before B and a later query orders B before A, offset one can return A after A was already seen. This is a model counterexample to an unspecified tie order. It is not a reproduced statement about the complete upstream SQL ordering.

A proposed total order can compare name and then a unique key. The storage query and resume predicate must use compatible comparison rules. Database collation, case equivalence, and key ordering still require inspection or integration tests. A C# model using ordinal strings cannot certify a database configured with different comparison semantics.

The source-backed conclusion is that the inspected service requests text ordering at its call site. The evidence gap concerns what additional ordering the lower layer supplies and what contract is promised. Good answers keep that gap visible instead of declaring a bug solely from the absence of a tie-breaker in one method.

## UM-16B: cursors improve some shifts, not every change

With A, B returned first, insertion of X before A shifts an offset boundary and can repeat B. A tuple cursor after B can avoid that particular repetition by asking for values greater than B's tuple. Deletion before the boundary similarly affects an offset differently from a cursor. These examples motivate continuation based on ordering values.

A rename can move an already seen entity after the cursor or move an unseen one before it. Without snapshot semantics, the cursor can still repeat or omit identities over a changing traversal. A picker may tolerate this with refresh and identity-based selection. A fixed export may require a stable result set or another stronger mechanism.

The answer should state the use case. It should not call a cursor an exact-once traversal guarantee unless the storage and mutation rules support that claim. A useful comparison table includes unchanged data, insertion, deletion, and rename so that the improvement and remaining limits are both visible.

## UM-16C: validate continuation as part of the API

A proposed cursor binds the query interpretation, object type, ordering version, relevant caller scope, and last examined candidate position. Depending on the design, it also binds a snapshot identifier or server-side result-set reference. Page-size changes need an explicit rule. An altered token should not be treated as trusted state merely because it can be decoded.

Define expired and query-mismatched tokens as documented failures or restart requirements. Do not silently reinterpret them as the first page. Missing hydration should not prevent progress past examined candidates under a candidate-based cursor, although that choice means a later-available record may not appear in the same traversal.

Acceptance tests should cover a valid continuation, a mismatched query, and a final candidate that fails hydration. Integrity and expiration tests belong to the chosen token mechanism. Ordering equivalence and database resume predicates require storage evidence. The current skip/take endpoint remains unchanged by this design exercise.

## UM-17A: apply one generation atomically

R1 starts at generation one for landing. R2 starts at generation two for news. When R2 succeeds, items and total become the news response and loading ends for generation two. When R1 later succeeds, its generation mismatch causes the transition to be ignored. The input, displayed items, and total still refer to news.

If R1 fails instead, the same guard prevents an old error from replacing the current success. Error handling needs the generation check just as success handling does. If R1 completes before R2 starts, it is current at that moment and should be applied normally. This positive case prevents a broken model that ignores every response from appearing correct.

Cancellation does not replace this guard. It may reduce work, but the client still needs a policy for late completion. A complete transition table includes loading and error fields, not just items, because stale failures and stale loading updates can confuse the interface even when the visible list remains correct.

## UM-17B: retain both counts

For total twelve and two returned items from a requested candidate page of five, the model stores total twelve and an item collection of length two. Navigation advances according to the candidate-page contract, not by two. Display wording should avoid implying that two is the total number of matches.

An empty visible page with a positive total should remain distinguishable from an empty search universe. A refresh option or navigation rule can be defined, but the model should not assert deletion or reset Total without evidence. The current controller preserves the search total even when hydration removes candidates.

The strongest answers include both ordinary and sparse examples. They also keep each response's total and items together under one generation guard. A correct server count becomes misleading if the client combines it with items from another query.

## UM-17C: keys, indexes, and focus are different

The selected value remains B after the order changes to C, A, B. A row-index selection would move to A or another entity depending on indexing conventions, which violates an identity-based selection contract. Keyboard focus can follow B if it remains present, but it should do so through key lookup rather than reusing the old position.

If B disappears from the current page, the design must decide whether to retain an out-of-view selection, mark it unavailable, or clear it with an explanation. Absence from one search page alone does not prove deletion. Focus may move to a search input or another explicit fallback without changing the selected entity silently.

These are proposed client behaviors. Applying them to the actual back office requires inspecting the component, its state management, and its accessibility behavior, then testing the integration. The rubric rejects claims that the real UI has been fixed by a pure state model, while rewarding a model that exposes the intended behavior precisely.

## Consolidated review rubric

Award the strongest assessment when a submission distinguishes current source, pure models, proposed integration tests, and deployed behavior. It should use synthetic fixtures, identify every relevant boundary, and demonstrate at least one counterexample to a plausible wrong implementation. A passing happy path alone is not enough for topics involving inheritance, continuation, or response races.

Ask the learner to defend one rejected alternative. For example, explain why sorting hydrated names changes the ordering authority, why advancing by emitted count confuses offsets, or why cancelling an old request does not prove it cannot complete. This defense reveals whether the learner understands the causal mechanism rather than memorizing a recommended pattern.

Finally, inspect the evidence labels. No chapter in this group implements a new authorization policy, snapshot transaction, refill endpoint, cursor, or client component. A complete exercise submission may propose or prototype those features, but it must not describe them as already present in Umbraco management API code or in the template-search action. Accurate scope is part of correctness, not an optional reporting detail.
