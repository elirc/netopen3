# Review guide: build evidence, isolated labs, and capstone assessment

Attempt chapters 19-23 before consulting this guide. The first useful hint is to put every claim next to the layer that could establish it. A source method, an extracted program, a direct controller test, and a hosted API request are related artifacts, but they execute different boundaries. The worked answers below preserve those differences and provide complete review criteria for both capstones.

## UM-19A: the graph contains several kinds of arrows

The outer TemplateOrder.Tests project compiles the linked upstream SearchTemplateItemControllerTests file and references the real Management API project. Its PackageReference entries obtain versions through the adjacent central package file, which imports the upstream test package configuration. The nested global.json supplies an SDK policy when invocation occurs under that checkout. These are different relationships, not interchangeable dependencies.

A Compile Include arrow means source participates in the test assembly. A ProjectReference arrow brings another project's output and build graph. An Import arrow evaluates configuration. SDK selection chooses the toolchain before those projects are built. A diagram that represents all four as copied files would mislead a learner about what changes when upstream source or configuration changes.

The focused test selection limits which tests are intended to execute; it does not remove every dependency required to compile the referenced production project. This explains why a small test file can still encounter substantial restore or build work. The answer should not promise a lightweight run simply because only four tests are present.

## UM-19B: report the stage that failed

For an unavailable SDK, record the requested SDK policy, the command context, and the fact that compilation and test execution did not begin. The next action is to inspect the installed toolchain and environment, not to change the selection algorithm. A missing toolchain is not a failed ordering assertion.

For a referenced-project compile error, report that project evaluation or restore may have completed but behavioral tests did not produce evidence. Identify the relevant compile failure without dumping unrelated logs or credentials. The next action targets the compile incompatibility or source state. A statement such as zero tests failed is misleading if no tests ran.

For one failed duplicate-payload assertion after four tests are discovered, the evidence is behavioral at the controller-test layer. Inspect whether the helper kept the first or last hydrated payload and whether mapping preserved that observation. Changing package pins is not the first causal response to this failure. The report should include the failing assertion and the fixture's intended payload winner.

## UM-19C: two manifests with different scope

A focused controller manifest should identify the command, working directory, selected SDK, relevant revision or source hashes, project and package configuration, discovered test count, exit status, and result artifact. Local modifications matter because the tested bytes may differ from a named commit. The record does not need secrets, remote credentials, or an inventory of unrelated user files.

The extracted-method manifest is smaller: source-file hash, extracted-method hash, copied-method hash, deliberate mutation if any, generated project, SDK result, and check output. It supports claims about that method body under the minimal entity contract. It does not replace a full project compilation record.

Full credit requires this difference in scope. A source hash is an identifier, not proof that the entire repository was tested. A passing isolated program is useful evidence, but it should not be described as all controller tests passing. The report should make it easy for another person to reproduce exactly the same boundary.

## UM-20A: restore consumption in the copied starter

The intentional starter changes Remove to TryGetValue. With requested B, A, B, A and hydrated A and B, lookup returns a value on every occurrence, so four items are emitted instead of two. The reference removes each dictionary entry when it is emitted, causing later occurrences of the same key to find no available entry.

A direct repair restores Remove in the disposable Selection.cs. The signature remains unchanged and the application source is untouched. The expected starter result is one behavioral failure concerning duplicate requests; after repair, all six checks should pass. The learner must record actual output rather than treating this expected result as execution evidence.

Keep the original SOURCE-MAP.json so the deliberate mutation remains visible. A repaired copy may no longer match its copied-method hash, which is useful historical information rather than a reason to falsify the original record. Add a separate repair note or new hash if documenting the repaired artifact. Do not rewrite provenance to pretend the starter was always the reference.

## UM-20B: choose the layer that executes the claim

First hydrated object wins is directly testable in the extracted-method lab with two equal-key objects and reference assertions. Response Total staying nineteen requires controller orchestration because the helper never receives Total. Authorization admission requires the configured authorization pipeline or equivalent effective-policy evidence; directly invoking Search bypasses that boundary.

Equal-name paging in the configured database requires query and storage evidence, including comparison semantics and ordering. JSON field names require the mapping and serialization configuration relevant to the actual response. A mocked mapper producing a desired object does not prove the real mapper or serializer emits the same shape.

The minimum useful layer is not always the most expensive layer. Use a small method test for a collection rule and a hosted or storage-aware test for the concerns it actually owns. An answer that insists every claim needs a deployed site is as imprecise as one that claims the isolated helper proves everything.

## UM-20C: make an independent wrong implementation fail

One useful extra fixture tests a selector that preserves requested key order but replaces duplicate payloads with the last object. Use first and later objects for A, request A once, and assert the selected reference is first. The provided suite already includes this principle, so a learner extending it should choose another distinct counterexample or strengthen an uncovered dimension rather than adding an identical assertion under a new name.

A different fixture can check that unrequested hydrated entities are never emitted even when their keys compare before requested ones under a sorting implementation. Another can use all requested keys missing with several extras, expecting an empty result. Write the expected sequence manually and explain the wrong algorithm it rejects.

The rubric rewards causal specificity. A random large fixture may find a defect, but a three-entity example that isolates the wrong rule is easier to review. Passing an additional fixture still does not establish HTTP policies, Total, or database behavior. The evidence ceiling remains the extracted method.

## UM-21A: a complete observable-reconciliation design

A suitable prototype returns selected payloads plus a diagnostic record whose fields have explicit occurrence or distinct-key semantics. It retains first hydrated payloads and emits each requested key once in requested order. It computes counts without re-enumerating a one-shot source. The mixed fixture produces A-first and C, while classifying B as absent, the second requested A as repeated, the second hydrated A as duplicate, and X as extra.

The prototype source map identifies which code came from the current helper and which diagnostics were newly authored. The application remains unchanged. A test matrix covers simple rules separately and then combines them. The record should include both selected references and diagnostic values so a correct count cannot hide changed payload semantics.

Memory analysis should mention the original hydration dictionary and output list plus any additional requested or emitted sets. It need not claim a benchmark that was never run. A reasoned complexity account and bounded synthetic tests are enough for the prototype, with performance validation identified as a later integration task.

## UM-21B: mutation evidence strengthens the tests

Changing TryAdd semantics to overwrite should fail the first-payload test even if key order remains correct. Counting every failed Remove as missing hydration should fail a fixture where a repeated requested key was already emitted. These mutations expose different rules and therefore need different assertions.

An independent oracle can traverse distinct requested keys in first-occurrence order and find the first matching hydrated payload in a tiny materialized fixture. It is intentionally simple and may be slower than the dictionary implementation. Its value is that it expresses the specification differently, reducing the chance that both implementations share the same accidental logic.

Restore the prototype after mutation experiments and record the final passing state. Do not leave the application or the reference extraction altered. The report should identify which failures were intentional mutation checks and which run represents the final prototype. A red console line can be successful evidence when the test was expected to reject a deliberate defect.

## UM-21C: rollout is a separate boundary

A conservative rollout keeps the public response shape and selection contract unchanged while adding internal observations. It defines counter names, cardinality, sink-failure behavior, and overhead limits. It avoids logging raw queries or payloads when counts are sufficient. A rollback condition could involve unacceptable overhead or diagnostic delivery affecting request behavior, but the threshold must be chosen for the actual environment rather than invented as a universal constant.

An additional controller test should verify that instrumentation does not rewrite Total or change the empty-search short circuit. Operational validation should inspect real telemetry volume, cardinality, and delivery behavior under the deployed configuration. Neither is established by the pure selector prototype.

The full solution includes a compatibility argument: selected item references and order remain the same, missing and extra entities remain excluded, and Total remains the search value. If any of those change, the proposal must be reviewed as a behavioral API change rather than described as harmless diagnostics.

## UM-22A: choose a promise you can implement

For a picker, a defensible model uses bounded candidate traversal, explicit sparse outcomes, and a client generation guard. It tolerates dataset changes and does not promise a historical snapshot. Candidate progress is independent from emitted count, and leftovers from a partially consumed batch have a defined disposition.

For a stable export, the model must represent a fixed membership mechanism rather than merely a tuple cursor over changing data. A stored synthetic result list can model that contract, but the report must identify the real persistence or snapshot capability still needed. The in-memory list does not prove the production database supports the required lifetime or isolation.

Both routes need finite work limits and a termination argument under permanent sparsity. A loop that stops only after collecting enough entities is incomplete when every hydration attempt misses. A clear budget-limited outcome is more honest than reporting ordinary completion after silently abandoning work.

## UM-22B: guard continuation as well as items

The client generation check applies to the whole response state: items, Total, continuation, loading, and error. If an old response cannot replace items but can replace the continuation token, the next-page request may still resume the wrong query. The state update must preserve association among all fields.

The required schedule starts R1, starts R2, applies R2, then delivers R1 success or failure. Neither stale outcome changes current state. A positive schedule applies R1 when no newer request exists. Selection remains keyed by identity across reorder, while focus follows an explicit policy. Absence from one page is not treated as proof of deletion.

A complete solution records these transitions in a table or executable model. It does not rely on cancellation to make stale completion impossible. Cancellation and generation solve related but different problems: reducing work versus deciding which completed work still represents current intent.

## UM-22C: compatibility includes invalid state

The rollout note states how new and old clients interact, how tokens bind query and ordering, how expiration is reported, and what happens during rollback. Invalid state should not silently become page one unless that behavior is explicitly chosen and its duplicate risk accepted. A documented restart outcome is often easier to reason about.

One rejected alternative might be unbounded refill, rejected because work must remain finite under missing hydration. Another might be server-side buffering, rejected because its lifetime and ownership cost is unnecessary for a simple picker. If no product requirement justifies continuation complexity, retaining ordinary sparse paging is a valid and often strong conclusion.

The solution should not call the existing endpoint cursor-enabled. The capstone produces a proposal and model. Real query predicates, authorization binding, token integrity, and UI integration remain separate implementation tasks until actually completed and tested.

## UM-23A: explain the full current path

A concise defense begins with requested keys B, A, B, missing and hydrated A-first, B-first, B-later, X. The helper retains the first A and B payloads, emits B then A, skips the repeated B, omits missing, and excludes X. The controller retains the search total and maps the selected entities.

An empty search page bypasses hydration and mapping. A nonempty page whose keys all miss hydration still reaches mapping with an empty selected sequence. The final item count alone cannot distinguish these branches. This is a useful example of why collaborator assertions can add evidence beyond response-shape assertions.

The defense should end with an honest limit, such as no hosted authorization test or no database ordering experiment. Name the next layer needed rather than apologizing vaguely that more testing is required. A precise boundary makes the existing evidence easier to trust.

## UM-23B: four regressions need four explanations

Sorting hydrated entities does not necessarily exclude extras or collapse duplicate payloads. A fixture with one requested key and one extra exposes membership drift. Last-payload insertion is exposed by two equal-key objects with different markers. Selected-count Total is exposed by a page with two returned items and a search total of nineteen.

An unobserved cancellation parameter is an evidence gap about execution control. A signature alone does not prove that any stage stops work. A useful test needs a controlled observation point and an expected cancellation outcome at the appropriate layer. It should not infer an HTTP status from a helper exception.

The review table should classify the first two as selection semantics, the third as controller metadata semantics, and the fourth as cancellation behavior requiring actual observation. This separation helps a maintainer choose focused tests and avoids treating every issue as a generic ordering bug.

## UM-23C: a handoff separates finished work from proposals

The handoff names the prototype files and source hashes, states its implemented model behavior, lists actual checks, and identifies application integration still proposed. It includes one compatibility concern such as Total preservation, token invalidation, or diagnostic sink behavior. It also records the chosen product promise so a future learner does not accidentally strengthen it while reusing the code.

A strong handoff can be followed without the conversation that produced it. Another engineer can locate the source, regenerate the lab, understand intentional starter failures, and decide which integration test comes next. It does not describe a pure model as a deployed feature or count an expected result as an executed test.

## Final scoring guide

Assess source accuracy, contract precision, counterexample quality, execution evidence, and scope honesty separately. A submission can have clean code but an unsupported snapshot claim, or good prose but no test that rejects a plausible defect. Give targeted feedback on the missing dimension rather than treating all weaknesses as insufficient detail.

The highest-quality work remains useful when challenged. It can explain why a dictionary entry is consumed, why first payload wins, why Total differs from item count, why a cursor is not automatically a snapshot, and why direct invocation does not execute authorization. Those explanations connect implementation details to observable behavior and make future changes safer to review.
