# Review guide: enumeration, completion and mapping boundaries

Use this guide after chapters 06–08. These exercises are about claims that sound reasonable but exceed their evidence: an enumerable must be a database query, an async method must perform asynchronous I/O, a mapper mock must verify real mapping, or an empty response must have skipped every collaborator. Each claim needs a precise execution boundary before it can be evaluated.

## UM-06A: what an enumeration counter establishes

Begin by naming the sequence being instrumented. Search Items is one sequence. Hydrated templates is another. The selected list and mapped responses are further collections. A wrapper around one of them does not automatically count visits to the others. Label each counter with its owner and record where the wrapper is injected. Otherwise a result such as enumerated twice can be attributed to the wrong component.

The action asks whether search Items has any elements and, on the nonempty path, extracts keys with Select and ToArray. For a general IEnumerable, those operations can involve separate enumerations. A deliberately one-shot or stateful enumerable is therefore a useful assumption probe. It shows how the controller behaves when its collaborator supplies that kind of sequence. It does not establish that the actual EntitySearchService issues two database queries, because the inspected implementation materializes its results into an array before returning them.

This distinction matters when proposing a performance fix. Removing an enumeration of a small array can reduce some traversal work, but it is not the same improvement as eliminating a database round trip. State the existing implementation, the alternative implementation under test and the measured operation. A benchmark that substitutes a costly lazy query for an actual array may reveal future fragility while exaggerating the current cost if presented without that context.

For the hydrated input, the helper builds a dictionary in one traversal. Requested IDs are already an array. An instrumented hydration enumerable can verify the helper's traversal behavior for a fixture without changing the repository. A source-level observation and a runtime counter complement one another: the source explains why a traversal occurs, while the counter can catch an accidental repeated enumeration introduced by a refactor.

## UM-06B: materialization is a tradeoff, not a slogan

A proposed controller refactor can materialize search Items once, test the resulting array's length and derive keys from that array. This gives those two operations a consistent local sequence and accommodates one-shot enumerables more predictably. The proposed change should be tested with empty, ordinary and stateful inputs. If the existing concrete service already returns an array, the refactor may add a copy unless it detects a suitable materialized representation. That allocation belongs in the review.

Local materialization does not create a transaction spanning search and hydration. A template can still change after its slim entity was captured and before its full model is read. The snapshot is of the returned sequence at one point in the controller, not necessarily of the database state underlying every stage. A learner who uses the word snapshot should immediately say what was captured and which later changes remain possible.

Do not silently deduplicate while materializing unless that policy is intentional. The current key extraction retains requested duplicates. The helper later emits each available key once. Deduplicating earlier might appear output-equivalent for selected models but changes the keys passed to hydration and may affect collaborator expectations or diagnostics. A safe refactor reviews both the final response and the interaction contract.

The empty branch must remain observable. If the refactor accidentally hydrates an empty array before returning, the final response can look unchanged while the service-call contract changes. That might be harmless or costly depending on the service, but it should be a deliberate choice. The regression's Never assertions make the present behavior clear and provide a useful guard during materialization experiments.

## UM-06C: deferred mapping requires actual evidence

The mapper interface returns a List for MapEnumerable. That return type is evidence about the result shape, but it does not by itself describe every detail of how the input is traversed or maps are invoked. To make an implementation claim, inspect the real UmbracoMapper in the infrastructure mapping directory. To make an observation about the controller test, inspect the configured mock callback. Those are different artifacts with different evidential scope.

A hypothetical mapper that defers work until later enumeration would introduce lifetime and mutation questions. However, a lesson should label that as a hypothetical design unless the actual mapper behaves that way. Do not invent deferred execution merely because the input parameter is IEnumerable. An enumerable input can be eagerly consumed into a list immediately.

A useful submitted experiment records when the input's enumeration counter increments relative to the MapEnumerable call and when the output is subsequently read. If the exercise uses a stand-in mapper, state that it demonstrates a language or interface assumption rather than verifies the production mapper. The resulting lesson remains valuable because it teaches how to investigate execution timing without inflating the claim.

## UM-07A: reconstruct the actual completion sequence

The search service call completes synchronously before keys are extracted. The controller then calls the template service's Guid-key overload and awaits its Task. In the inspected implementation, repository access occurs while producing the result that is wrapped with Task.FromResult. Ordering and mapping follow completion of that call. A diagram should show that sequence without inventing worker threads or remote requests.

Await expresses how the caller obtains completion. It can suspend if the awaited operation is incomplete, but an already-completed Task need not introduce a meaningful asynchronous wait. The action's async signature therefore cannot be used as a performance claim on its own. To discuss blocked threads or asynchronous database access, inspect the concrete dependency and measure the relevant workload rather than reasoning from the method name.

Different test doubles can exercise different completion behavior. A completed Task makes the ordinary composition test simple. A controllable incomplete Task can show that mapping does not occur before hydration completes. A faulted Task can show that the action does not return a successful mapped response after a dependency failure. These tests establish ordering and propagation within the direct call; they do not measure production database scheduling.

Record events such as search called, hydration started, hydration completed and mapper called. Check the event order rather than adding arbitrary sleeps. A test that waits a fixed number of milliseconds can pass or fail because of scheduling noise. A manually controlled completion source allows the test to assert the pre-completion state and then release the dependency explicitly.

## UM-07B: a token parameter is not cancellation coverage

The action accepts a CancellationToken, but the inspected body does not pass it to the search or hydration dependencies and does not explicitly check it. A direct invocation with an already-canceled token therefore investigates the action body, not the entire HTTP request lifecycle. It would be inaccurate to describe that test as proof that the web server ignores disconnected clients everywhere.

An improved cancellation design has several possible contracts. It could reject work before search begins, propagate cancellation into a dependency that supports it, or check after hydration before mapping. These choices stop different amounts of work. Adding a check after hydration cannot retroactively cancel the repository operation that already completed. A proposal should identify the boundary it affects and the remaining work it cannot prevent.

Cancellation also interacts with errors. A dependency may fail for an ordinary reason at roughly the same time the request is canceled. The desired observable outcome depends on where cancellation is checked and what the framework does with the resulting exception. Avoid promising a particular HTTP status based only on a direct-action exception assertion. Hosted integration evidence is needed for the public response claim.

For the exercise, write two columns: present source behavior and proposed cancellation behavior. In the first, list the unused action token and current dependency signatures. In the second, list the exact checks or overload changes needed. This format prevents a learning plan from being mistaken for an implemented feature and makes the migration work reviewable.

## UM-07C: why Task.Run is not a universal repair

Moving synchronous repository work into Task.Run changes where that work executes. It does not inherently make the repository operation cancellable or turn synchronous database access into asynchronous I/O. If the operation holds a scope or uses contextual services, moving execution also requires understanding those lifetimes. A convincing proposal must address the actual dependency contract rather than relying on an async-looking wrapper.

Consider a request canceled immediately after its work is queued. A cancellation token associated with scheduling can sometimes prevent work from starting, depending on the API use, but work already executing needs cooperative cancellation within the operation to stop promptly. The exercise should distinguish those moments. A broad statement that passing a token cancels the query is unsupported unless the query itself observes it.

There may be legitimate reasons to offload particular CPU-bound work, but this source trace does not establish such a workload. A reviewer should ask for the measured bottleneck, expected concurrency and lifecycle model. The simplest correct outcome of this exercise may be a documented limitation and a targeted dependency-level improvement plan, with no production change yet.

## UM-08A: expected records must preserve associations

Use requested keys C, A, B and hydrated A-first, B-first, A-later, C-first. Give every instance a distinctive alias and name. The selected instances are C-first, A-first, B-first. The real template item mapping copies each selected Key to Id, its Alias to Alias and its Name to Name with an empty-string fallback for null. The response tuple for A must therefore carry A-first's values, not A-later's values or B-first's name.

An assertion on IDs alone verifies only part of that story. An assertion on independent sets of IDs and aliases still fails to establish their associations. Compare ordered records or ordered pairs whose expected values are written independently. This catches a mapper that zips IDs and aliases from different sequences while preserving each set individually.

The existing controller regression mapper mock projects ID and Alias. It is appropriate for observing which selected templates the controller supplies to mapping. It does not execute the real map definition or verify the Name fallback. To claim those behaviors, use a real-mapping test or source inspection clearly labeled as inspection. Do not silently broaden the mock test's result into proof of production mapping configuration.

The required Alias declaration on the response model is a construction requirement at the C# model level. It is not sufficient evidence that every runtime input is meaningful, nonempty or validated at an HTTP boundary. If an exercise explores empty aliases, first locate the relevant domain or serialization constraints. Do not substitute a language keyword for a complete data-validation contract.

## UM-08B: equal outputs can hide different paths

Case one starts with an empty search Items sequence and Total seventeen. The controller returns an empty response and skips hydration and mapping. Case two starts with requested keys A and B, but hydration yields no matching templates. The helper returns an empty list, the mapper is invoked with that list, and Total remains the search-reported value. Both responses can contain zero items, yet the interaction traces differ.

This matters when injecting failures. A mapper configured to throw is irrelevant to the first path because it is never called. It can affect the second path. A test that asserts every empty response skips mapping would therefore encode a false generalization. Base call expectations on the branch condition in the source: empty search Items, not empty eventual mapped Items.

Negative-call assertions are especially useful when an optimization is part of the contract. They show that a dependency is avoided, not merely that its result is discarded. Keep them narrow enough to survive reasonable refactoring. For example, verifying that template hydration is never called on empty search is stronger and clearer than globally forbidding every unrelated collaborator invocation in the fixture.

## UM-08C: isolate mapping without copying it

A real-mapping test should use the repository's registration mechanism and the actual ItemTypeMapDefinition. Find an existing mapping test and reuse its setup pattern before building a new bootstrapping system. The exercise is to execute production configuration with a small input, not reproduce the mapping lambda in the test and then compare it with itself.

If constructing the real mapper pulls in more infrastructure than the exercise can reasonably manage, record that limit. Retain the controller mock test for composition, cite the mapping source for the field assignments and propose a later focused integration test. This is a complete learning outcome when the evidence is explicit. It is more reliable than a lightweight imitation presented as if it executed the actual CMS map.

Finish with a boundary table containing the helper, controller, mapper and hosted API. State which layer each fixture exercises and which behavior it cannot establish. That table should make it impossible to confuse a successful direct call with verified authorization, serialization or request cancellation. The discipline transfers to other frameworks because every test runs within a chosen environment, and that environment limits the conclusion.
