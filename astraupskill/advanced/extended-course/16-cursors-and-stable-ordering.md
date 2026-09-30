# 16. Cursors and stable ordering

Offset paging is easy to describe: skip a number of matching candidate rows and take a batch. It becomes harder to reason about when the matching set changes between requests. This chapter examines that limitation and develops a proposed cursor design without pretending that the current search endpoint already supports one. Its prerequisite is the distinction between candidate identity order and hydrated payload order from the earlier chapters.

The inspected [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs) asks for paged descendants with an ordering based on node text. It materializes the returned slim entities. That call site does not itself declare a unique secondary ordering key. Do not infer the complete SQL ordering or database tie behavior without inspecting the lower layers. The safe observation is that the service explicitly requests text ordering at this boundary.

## Equal sort values need an ordering decision

Imagine templates A and B both have the displayed name Landing. A third template C is named News. A page boundary cuts between the two Landing records. If the complete ordering does not distinguish A and B, their relative positions may not be stable across queries. An ordering contract intended for reliable continuation should specify a unique tie-breaker, such as the entity key, after the text sort value.

Adding a tie-breaker is a proposed change with consequences. The database comparison for names and the serialized cursor comparison must agree. Case handling, culture, collation, and null semantics cannot be guessed from ordinary C# string comparison. If the database orders two names as equal while cursor logic treats them as unequal, a resume predicate can skip or repeat rows. The design needs one authoritative comparison definition.

The unique key also needs a defined ordering representation. It is insufficient to write name then GUID without checking how the storage provider orders keys and how the cursor predicate is constructed. The goal is a deterministic total order over the candidate set, not a visually plausible sort. A concrete integration test with equal names is more informative than a large fixture where every name happens to be distinct.

## Worked offset shift

Start with candidates A, B, C, D in stable order and a batch size of two. The first request returns A and B. Before the next request, a new candidate X is inserted before A. The new ordered set is X, A, B, C, D. An offset of two now starts at B, so B appears again. If a record before the boundary is deleted instead, an offset can move past an unseen record.

These are logical examples of changing membership, not evidence of a specific production race. They explain why a client cannot derive stable traversal merely by incrementing skip. The current controller's ordering helper can correctly preserve each page's search order while this cross-request behavior remains possible. A helper that only sees one page cannot repair movement of the global page boundary.

A cursor might represent the last candidate sort tuple, such as the last name and key under an agreed comparison. The next query asks for candidates strictly after that tuple. An insertion before the tuple would not shift the starting position. However, a rename can move a previously seen entity after the tuple or move an unseen entity before it. Cursor paging improves some behaviors; it is not automatically a snapshot of a changing dataset.

## Define what a cursor promises

Choose among several possible promises. A traversal cursor may guarantee progress through the current ordering while tolerating changes. A snapshot cursor may refer to a stable result set with stronger replay semantics. A short-lived server-side cursor may keep a specific candidate list. These designs have different storage, expiration, and authorization requirements. The word cursor alone does not identify the guarantee.

For a template-picker interface, a modest contract might be acceptable: continue after the last candidate tuple, allow concurrent renames to affect later results, and deduplicate selected items in the client by identity. For an export that must include each matching entity exactly once from a fixed moment, that contract may be insufficient. Product purpose should determine the consistency requirement before a token format is designed.

If the cursor represents candidate progress, missing hydration should not reset it to the last emitted entity. Consider candidates A, B, C where only A hydrates. Resuming after A can repeatedly examine B and C. Resuming after the last examined candidate makes progress but may never show B if it becomes available later. Both facts belong in the contract. Refill and cursor design therefore share the same distinction between examined candidates and emitted items.

## Bind continuation to its query

A continuation token should not be reusable with an unrelated query unless the API explicitly defines that behavior. A proposed token can bind the query interpretation, object type, ordering version, page policy, and relevant caller scope. If a token issued for one search is accepted with another search, the resume tuple may have no coherent meaning in the new candidate set.

Do not place secrets or unnecessary template payloads in a client-visible token. Encoding is not confidentiality. A signed token can protect integrity without hiding its contents, while an opaque server-side identifier introduces storage and lookup requirements. The design must choose the mechanism appropriate to the actual information and threat model. This workbook does not implement either mechanism or assert that the current endpoint uses tokens.

Expiration and invalidation also need observable behavior. If a cursor expires, the client should receive a documented outcome and a way to restart the search. Silently treating an invalid token as the first page can create duplicates and confusing navigation. If a query or authorization context changes, starting a new search is often clearer than attempting to reinterpret the old continuation state.

## Test a small model before a storage implementation

Use a disposable sequence model with tuples containing a name and synthetic key. Define its comparison explicitly and test resume after equal names, insertion before the boundary, deletion before the boundary, and rename across the boundary. The model can reveal a missing tie-breaker or a confused continuation position. It cannot prove that a database uses the same collation or predicate translation.

The next layer is an integration test against the actual query implementation proposed for the feature. Compare model predictions with storage results for deliberately difficult values. Include equal names, name changes, and the last candidate failing hydration. The test should record whether the cursor is based on candidate position or emitted position. A test that only traverses an unchanged list of uniquely named records leaves the most important ambiguities untouched.

Avoid implementing a new cursor by sorting all results in application memory without evaluating scale. That can replace a bounded page query with a full scan. The learning prototype may use a small in-memory list, but its limitations must remain explicit. Production continuation usually needs support from the query layer to preserve both correctness and bounded resource use.

## Exercise UM-16A: expose an unstable boundary

Construct a two-page example with duplicate names. Show how an unspecified tie order can change membership at the page boundary even when the set is otherwise unchanged. Then add an explicit unique tie-breaker in your proposed contract and state what still depends on database comparison semantics. Do not claim that the inspected service definitely emits unstable results; distinguish the missing evidence from a reproduced defect.

## Exercise UM-16B: compare offset and cursor promises

Use insertion, deletion, and rename between requests. For each mutation, predict the behavior of an offset and a proposed tuple cursor. Identify one case the cursor improves and one case it does not make snapshot-consistent. Recommend a contract for an interactive picker and a different contract for a fixed export.

## Exercise UM-16C: design continuation validation

List the fields or server-side state a proposed cursor must bind. Define expired, altered, and query-mismatched token behavior. Explain how missing hydration affects progress. Provide three acceptance tests and identify which tests require real storage. Keep the proposal separate from the existing skip/take endpoint.

## Review checkpoint

A strong design specifies a total ordering, query binding, progress semantics, and invalidation behavior. It does not use cursor as a synonym for snapshot. It recognizes that ordering one returned page and continuing across a changing dataset are different engineering problems with different evidence requirements.
