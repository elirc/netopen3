# Review guide: test strength, shared callers and input meaning

Read this after attempting chapters 09–12. A useful test suite does more than pass against the current implementation. It rejects plausible incorrect behavior, makes its own assumptions visible and avoids claiming results beyond the environment it executes. These exercises combine fixture design with repository navigation so that a learner can propose improvements without accidentally changing a wider contract.

## UM-09A: argument forwarding needs an observation

The existing controller tests configure search with broad argument matchers. That allows the test to supply a prepared search result but does not by itself prove that query, skip or take were forwarded correctly. Add a fixture with a distinctive query and nondefault paging values. Record the actual arguments or verify them explicitly. Also verify the template object type, because a controller that searched another entity type could still receive the configured response from an overly permissive mock.

Choose values that differ from one another and from defaults. A query such as learning-template, skip two hundred and take fifty makes accidental default substitution visible. The example is about forwarding, not a hosted endpoint's accepted input range. Since the real paging helper requires skip to be a multiple of take, using compatible values keeps the fixture focused on its intended concern rather than mixing it with a different input error.

The hydration argument should preserve the extracted key sequence, including duplicates at this stage. Requested B, A, B should be passed as that sequence under the current action. Selection later emits each available key once. If a refactor deduplicates before hydration, final output tests might still pass while the interaction changes. Decide whether that change is acceptable explicitly rather than allowing a broad matcher to hide it.

A strong submission contains both positive output assertions and interaction evidence. It verifies the returned ordered records and the arguments supplied to search and hydration. Keep implementation details out of the assertion unless they are part of the behavior under review. There is no need to assert local variable names or the exact sequence of harmless internal assignments.

## UM-09B: dependency failures reveal the call boundary

Inject a failure at search, hydration and mapping in separate fixtures. Search failure should prevent later stages from obtaining their required inputs. Hydration failure should prevent mapping a successful selected result. A mapper failure occurs after selection. For each case, state the direct-call observation: which exception or task failure is visible and which later collaborators were not called. Do not infer a production HTTP error envelope from those unit-level results.

The action body does not contain a local recovery strategy that silently substitutes a success response for those failures. A test can protect that orchestration fact without specifying what unrelated exception middleware ultimately does. If the learning goal is the HTTP response, use a hosted test that actually includes that middleware and the relevant filters. The test name should expose its level, such as direct action propagates hydration failure.

Avoid a single fixture that makes every dependency throw. Only the earliest reached failure will be observed, so the test does not establish the later failure paths. Independent cases make the causal chain clear. Similarly, an empty search page never reaches hydration or mapping, so configuring either to fail on that branch proves only that it was not called if you assert that explicitly.

A controlled asynchronous hydration fixture can hold completion while you verify that mapping has not started. Release the fixture with a failure and verify the final direct-call outcome. This tests ordering without arbitrary sleeps. The useful artifact is an event trace and expected call boundary, not a long timeout that happens to pass on one machine.

## UM-09C: broad assertions can certify the wrong thing

An assertion that response IDs are equivalent to expected IDs often ignores order. That is insufficient for an ordering regression. An assertion on response count ignores both membership and payload selection. An assertion that the result is assignable to a generic action result can establish its broad shape while missing an unexpected status or value. Select the assertion according to the failure it must reject.

The current controller derives from the management base and returns the expected OkObjectResult in the focused regression. Do not transfer an exact runtime-type assumption from another application merely because its action also calls Ok. Different base controllers can hide or wrap response helpers. Read the inheritance chain and use the assertion appropriate to this repository's actual result type.

Mutation thought experiments are useful before running any mutation framework. Ask whether the test would fail if the helper returned hydration order, emitted extras, chose the last duplicate payload, or recalculated Total from selected count. If the answer is no, identify whether another test covers that requirement. A suite can distribute coverage across cases; each individual case need not detect every mutation.

## UM-10A: construct an independent finite oracle

Use a small identity universe, such as A and B, with an extra identity X and distinct payload labels for duplicate instances. Enumerate short requested sequences and short hydrated sequences. The oracle can walk requested keys, skip those already emitted, and find the first hydrated model with each requested key using a simple linear scan. This is intentionally different from the production dictionary-and-removal mechanism.

The oracle's simplicity matters more than speed for a small universe. It should read like the contract: requested order, first hydrated payload and single emission. Do not copy the production dictionary code into expected-value generation. Two copies of the same mistake can agree perfectly. Mechanistic independence reduces that risk, although it does not eliminate the need to review the oracle itself.

Compare ordered payload identities, not only keys. If A-first and A-later share a key, their distinct labels expose which one won. Keep the payload labels stable during the experiment so that a failure can be reproduced from the printed fixture. Random generation can complement the finite universe, but a deterministic small enumeration makes coverage and reruns easier to explain.

Report the number of input pairs actually checked, the maximum sequence lengths and any exclusions. Do not call a finite experiment proof for all possible inputs. It provides systematic evidence over a defined domain. Its value comes from exercising combinations that hand-written examples may miss while retaining a transparent boundary around the claim.

## UM-10B: mutations should target independent requirements

Replace TryAdd with assignment to create a last-payload winner. Replace Remove with a non-consuming lookup to repeat requested duplicates. Return dictionary values to abandon requested order and potentially include extras. Set response Total to selected count to violate metadata preservation. Each mutation should have a small fixture that fails for a clear reason.

Keep helper mutations separate from controller mutations. Recalculating Total is a controller response change, not a property of the generic ordering helper. A helper-only harness cannot detect it because Total is not in its input or output. This is a useful reminder that even excellent algorithm tests cannot verify a surrounding orchestration layer they never execute.

If a mutation survives, first inspect whether it is behaviorally distinguishable within the defined contract. Some source changes are equivalent for all valid inputs. Others require a fixture absent from the current domain, such as duplicate payloads or empty search. Treat survival as a question to investigate, not automatic proof that a test suite is poor or that the mutation must be killed at any cost.

For a learning lab, a table of mutation, predicted counterexample and observed result is often more useful than a single percentage score. It teaches why a test exists and gives future maintainers a focused regression case. Preserve the failing input when a generated experiment finds a new issue.

## UM-10C: shrinking makes failure explainable

Suppose a large fixture fails because assignment chose A-later. Remove requested identities other than A and hydrated entries unrelated to A. Keep the two A payloads in their original order. The minimal counterexample has one requested key and two hydrated models with that key. Its output difference is now unmistakable: first payload versus last payload.

For repeated output caused by a non-consuming lookup, the minimal shape needs repeated requested A and one hydrated A. For an extra-leak mutation, an empty requested sequence and one hydrated extra can suffice, depending on the candidate implementation. For an ordering mutation, two available distinct keys in opposite requested and hydration orders make the contrast visible.

Shrinking must preserve the failure condition. Deleting the earlier duplicate in a first-versus-last example removes the distinction and makes the test pass. Record which property the reduced fixture still violates after each simplification. A smaller fixture is valuable only if it remains a faithful explanation of the original failure.

## UM-11A: a caller matrix prevents accidental generalization

List each shared-helper caller with its input collection, hydration source, mapping approach and Total meaning. SearchTemplateItemController uses search page keys and preserves search Total. BatchDocumentTypesController converts its input HashSet to an array, hydrates document types, applies the helper, maps selected items and reports the mapped count. Those response totals have different authorities even though the selection mechanism is shared.

For HashSet callers, state only that the helper preserves the supplied array order. Establishing the original HTTP query order requires separate model-binding evidence. A set's role in the action signature makes deduplication and ordering assumptions worth investigating. Do not use a helper test over a manually constructed array as proof of wire-level ordering guarantees.

Include at least one nearby controller that does not call the helper. ItemTemplateItemController hydrates its provided IDs and maps directly in the inspected source. Its existence prevents the false claim that every item endpoint shares the ordering policy. A repository-wide text search supplies candidate callers, but reading each relevant body is what establishes its actual response behavior.

The matrix should include uncertainties. For example, if you have not inspected a sibling's real mapper or service implementation, mark those cells as unverified rather than copying facts from the template path. Similar controller structure is a useful lead, not evidence that every dependency behaves identically.

## UM-11B: generic builders need explicit metadata policy

A response builder that accepts selected items and always computes Total from their count fits some batch responses but changes search responses. A builder that always preserves an upstream Total needs an upstream total argument and may be meaningless for a simple batch request. Shared code should expose the policy difference instead of hiding it behind a single default.

One design is to share only reconciliation and let each caller construct its response. Another is a builder whose arguments make Total authority explicit. Compare readability, accidental misuse and compatibility before choosing. The current helper already isolates a mechanism without owning every response concern, so broader abstraction needs a concrete benefit rather than a desire to reduce line count alone.

Write two examples before proposing the API: sparse search with Total nineteen and two selected models, and a batch with two mapped models whose Total is two. Any candidate abstraction should represent both without misleading names or conditional guesses based on collection size. This small design exercise catches a common abstraction failure early.

## UM-11C: duplicate policy is shared behavior

Changing first-wins to last-wins affects every caller whose hydration can contain duplicate keys, even if the motivating bug was observed only in template search. Inventory callers and investigate whether duplicate payloads are valid, impossible by a documented service guarantee, or merely uncommon. Absence of an observed duplicate in one integration test is not proof of impossibility.

A proposal might make duplicate handling explicit per caller, preserve the existing default, or reject duplicates at an earlier boundary. Each option changes the public or internal contract differently. Record migration tests and compatibility expectations. A shared helper's small size does not make a semantic change local in effect.

Review payload identity as well as key order in representative caller tests. If every fixture uses duplicate objects with identical data, a winner-policy change can pass unnoticed. Distinct payload markers make the behavior visible while keeping the test independent of unrelated domain complexity.

## UM-12A: query syntax selects a branch

The local entity-search implementation treats null or whitespace query as an unfiltered query, a parseable Guid as a key query, and other text as a name Contains filter. A Guid-shaped display name therefore follows the identity branch under this implementation. The query table should include each branch and explain the difference without assuming database case or collation behavior that has not been tested.

Leading or trailing text around a possible Guid should be evaluated according to Guid.TryParse and the actual supplied string, not a hand-written rule invented by the exercise. If exact parsing edge cases matter, run a focused language-level probe and label it as such. The repository source establishes which parser is called; the parser's detailed accepted forms can be investigated separately.

The name branch uses the supplied query in its expression. Do not describe it as tokenized full-text ranking or fuzzy matching without evidence of such behavior downstream. A user-facing search explanation should reflect the actual service used by this endpoint and distinguish proposals for richer search from present functionality.

## UM-12B: arithmetic evidence is narrower than HTTP behavior

PaginationHelper checks that skip is a multiple of take and then calculates page size and zero-based page number. A normal pair such as skip two hundred and take fifty gives page number four. A nonmultiple triggers the helper's ArgumentException. A zero take reaches modulo by zero because the helper has no explicit zero guard in the inspected code. Negative inputs need careful arithmetic inspection rather than an assumed positivity rule.

These are helper observations. They do not establish which HTTP requests survive framework validation, which filters execute or what error response a hosted API produces. The management base contains a paging-related problem helper, but its existence does not prove that SearchTemplateItemController calls it. Follow invocation sites before describing error translation.

Choose boundary cases with a stated purpose: default paging, exact multiple, nonmultiple, zero take and negative values. Keep expected helper results separate from expected endpoint responses. A learner can complete the arithmetic investigation without claiming a security defect or production outage from an untested request path.

## UM-12C: make a proposed contract reviewable

A clearer input contract might require nonnegative skip, positive take, an upper take bound and the existing multiple rule. Those are design choices to evaluate against current clients and framework behavior. Write the validation order, error representation and compatibility implications before implementation. If two inputs are invalid simultaneously, deterministic validation order helps make tests and client behavior understandable.

Select limits from workload evidence or product requirements, not arbitrary confidence. The course can propose a bounded exercise limit while clearly labeling it as a proposal. A production change needs an explanation of memory, database work and expected usage. Arithmetic safety and operational safety are related but distinct reasons for bounds.

Finish the review with a one-page evidence matrix. For each claim, identify source inspection, direct unit test, actual-source lab, hosted API test or unresolved investigation. The matrix should reveal exactly what your work proves. This habit makes technical reports more useful because readers can trust both the positive findings and the stated limits.
