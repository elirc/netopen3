# 21. Capstone: observable reconciliation without semantic drift

Build a reviewable proposal and disposable prototype for an observable template-selection boundary. The feature should explain sparse results while preserving the current contract: requested identity order, one emission per requested key, first hydrated payload wins, missing and unrequested entities omitted, and search total preserved by the controller. The capstone is complete as an instructional brief; the proposed feature is not installed in the application by this workbook.

Use [ManagementApiControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs), [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs), and the [actual controller tests](../../../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs) as the source boundary. The helper owns selection; the controller owns the response total and empty-search short circuit. Keep those responsibilities visible rather than moving every concern into one large method.

## The operational problem

An operator sees a page with fewer displayed items than requested. The current response alone may not reveal whether the reduction came from missing hydration, repeated requested identities, or another cause. Logging every template and query would create unnecessary data exposure and noisy evidence. The proposed feature should provide bounded, well-defined observations that help distinguish those cases without changing which items are returned.

The feature is not permission filtering, page refill, snapshot isolation, or a new search algorithm. Those can be separate proposals. Keeping this capstone focused makes it possible to defend behavioral compatibility. If a prototype also changes payload precedence or total semantics, a reviewer cannot tell whether a changed result comes from instrumentation or a new selection policy.

Write a one-paragraph problem statement with a concrete example: requested A, B, A, C, hydrated A-first, A-second, C, X, and total twenty. The response contains A-first and C in that order with total twenty. The diagnostic record should explain duplicate request, duplicate payload, missing B, and extra X as distinct categories. Do not call any of them deletion unless an additional source establishes that cause.

## Required deliverables

Produce a contract note, a source map, a disposable prototype, a fixture matrix, a result report, and a rollout proposal. The contract note defines every counter and the selected-output rules. The source map explains which code is copied or extracted, which parts are newly designed, and which original files remain unchanged. The fixture matrix includes counterexamples to plausible wrong implementations.

The prototype may be a small C# program using the existing lab as a starting point. Generate it into a new directory and keep the original extraction manifest. Add a separate diagnostic result type rather than modifying the public API response immediately. This allows you to test the semantics before deciding whether observations belong in internal metrics, structured logs, or a response field.

The result report must contain actual outcomes if you execute the prototype. If a test cannot run, record that limitation instead of copying an expected pass count. The rollout proposal remains a design artifact: it identifies compatibility, performance, and privacy checks needed before integrating the feature into the real application. A prototype passing synthetic cases is a useful milestone, not deployment approval.

## Design the result and observation boundaries

A possible prototype returns a pair consisting of selected entities and a diagnostic record. The selected collection should retain the same payload references as the current helper. Diagnostics can contain bounded counts rather than full entity data. Define requested occurrences, distinct requested keys, hydrated occurrences, duplicate hydrated occurrences, emitted entities, missing distinct requested keys, and extra distinct hydrated keys.

Be explicit about memory cost. The original helper uses a dictionary of hydrated keys and an output list. Additional sets for distinct requests or emitted keys add storage proportional to relevant input size. The capstone does not require a universal performance benchmark, but it does require a reasoned account of what is allocated and why. Avoid claiming zero overhead merely because the new fields are integers.

One design can collect hydration counts during dictionary construction and maintain a requested-key set for classification. Another can use a separate diagnostic pass over a materialized array, but then materialization and enumeration costs must be part of the contract. The current generic IEnumerable input makes repeated enumeration a meaningful compatibility concern. Prefer a design whose consumption behavior can be tested directly.

## Protect the winner and the order

The first hydrated object for a key must remain the selected object. A test with two equal-key payloads should compare reference identity or an unmistakable marker. If diagnostics use a dictionary assignment that overwrites the original value, the output can retain correct keys while violating payload precedence. This is a semantic drift that a key-only test misses.

Requested order remains authoritative. A proposal to sort selected items by name would be a different feature, especially when payload names changed after search. Instrumentation should not quietly repair a visual ordering concern by replacing the source contract. If a reviewer asks for a different ordering policy, write a separate compatibility proposal and distinct tests.

The controller's total should remain the original search total. The diagnostic record can report page-level counts without replacing that value. A page-level emitted count and a global matching total answer different questions. A feature that sets Total to the number of selected items may make a dashboard easier to read while breaking pagination semantics for clients.

## Fixture matrix and independent oracle

Include ordinary scrambled hydration, missing requested keys, unrequested extras, repeated requested keys, duplicate hydrated payloads, empty requested keys, and a one-shot enumerable. Add at least one mixed fixture containing several conditions together. The mixed fixture checks that independent counters do not interfere and that repeated consumption is not mislabeled as absence.

Write expected results by hand before implementing the prototype. For a second oracle, use a deliberately simple specification approach on tiny inputs: traverse distinct requested keys in first-occurrence order and find the first matching hydrated payload. This can be slower than the production algorithm while remaining easy to reason about. It should not copy the same dictionary-removal implementation, because matching bugs in two copies provide weak evidence.

Limit exhaustive exploration to a small synthetic key alphabet and short sequences if you add it. Enumerating all arrangements over three keys can expose duplicate interactions without generating a large workload. Record the input bounds and why they cover the intended counterexamples. Do not confuse finite exploration with a proof over arbitrary mutable or null-containing entity sequences.

## Faults and observation delivery

Decide whether diagnostics are returned as data or sent to a sink. Returning a record from the pure prototype makes failures easier to isolate. A later sink integration adds possible exceptions, blocking, batching, and drop behavior. State whether sink failure is allowed to affect the API result. For a best-effort diagnostic policy, a controlled sink exception should leave selected items unchanged while following a documented fallback.

Avoid hiding all exceptions in the selector under the name of telemetry resilience. An exception while reading an entity key is not automatically a sink failure. The proposed boundary should distinguish selection work from observation delivery so that only the intended failure category is handled. Broad catch blocks can convert real application defects into misleading successful responses.

Cancellation also needs scope. This helper is synchronous and has no cancellation parameter. Adding cancellation to a broader orchestration proposal requires explicit observation points and compatible signatures. The capstone can discuss that extension, but it should not claim that a metrics wrapper has made underlying storage operations interruptible.

## Privacy and operational interpretation

Keep default metrics low-cardinality. Endpoint name, bounded outcome category, and perhaps a coarse page-size bucket can be reasonable design candidates. Raw queries, template names, and one label per entity key are not needed to count reconciliation events. If debugging occasionally requires identities, propose a separate bounded diagnostic mechanism with a clear purpose and retention policy.

Define alert interpretation cautiously. A missing-hydration count establishes a mismatch between inputs, not its cause. A rise might motivate investigation into concurrent changes, service behavior, or data consistency, but the metric alone does not prove any one explanation. The operator-facing note should describe what to inspect next instead of issuing a confident but unsupported diagnosis.

A correlation identifier connects stages of a request. It is not a database snapshot identifier. Include this distinction in the operational guide so users do not assume that correlated search and hydration observations necessarily represent one moment. Good diagnostics make uncertainty easier to investigate; they do not erase it by renaming fields.

## Exercise UM-21A: build the compatible prototype

Implement the selected-output and diagnostic pair in a new disposable directory. Preserve reference identity, requested ordering, and one-time emission. Supply a source map and a mixed-fixture ledger. Demonstrate that one-shot hydration input is consumed according to your stated contract. Keep original application files unchanged.

## Exercise UM-21B: attack your own evidence

Introduce two temporary defects only in the prototype: last-payload wins and failed-removal counted as missing hydration. Show which tests reject each defect. Restore the prototype afterward and record the final result. Explain why the tests are independent enough to detect both problems rather than merely comparing two copies of the same implementation.

## Exercise UM-21C: prepare a rollout review

Write a proposal for integrating observations without changing the response shape. Include performance, privacy, sink-failure behavior, and a rollback condition. Identify one additional controller test and one operational validation that the pure prototype cannot establish. Clearly label the feature as proposed until that integration is implemented and verified.

## Completion rubric

The capstone passes when the prototype preserves all stated selection semantics, counters have unambiguous definitions, adverse fixtures reject plausible defects, and the report identifies its execution boundary. It does not pass merely because a dashboard looks useful or a test command exits zero. The strongest submission can explain exactly which behavior remains unchanged and why each new observation is worth its cost.
