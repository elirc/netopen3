# 18. Diagnostics without changing the contract

A sparse page can be correct under the current reconciliation contract and still deserve operational investigation. The challenge is to observe what happened without changing selection behavior, exposing unnecessary content, or measuring the wrong quantity. This chapter designs a bounded diagnostic extension around the actual helper. The extension is a proposal; the inspected helper does not currently emit these metrics or a diagnostic envelope.

Read [OrderByRequestedIds in the management base](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs). It builds a dictionary with TryAdd, then consumes matching entries with Remove while traversing requested identities. Those two phases make several distinct events observable. They do not provide a complete account of why a service omitted an entity, and a diagnostic design must not invent that reason.

## Name counters after observations

Useful candidate counters include requested occurrences, distinct requested keys, hydrated occurrences, distinct hydrated keys, duplicate hydrated occurrences, emitted entities, distinct requested keys without hydration, and distinct hydrated keys not requested. Each counter needs a precise definition. Without one, two implementations can report the same metric name with different meanings.

Consider requested A, B, A, C and hydrated A-first, A-second, C, X. Requested occurrences are four; distinct requested keys are three. Hydrated occurrences are four; distinct hydrated keys are three. There is one duplicate hydrated occurrence, two emitted entities, one distinct requested key without hydration, and one distinct hydrated key not requested. The repeated A request is not a second missing entity after A has been consumed.

That last distinction is easy to lose if diagnostics are added only around failed Remove calls. Both B and the second A fail removal, but for different reasons. B never existed in the hydration dictionary; A was emitted earlier. Labeling both as missing hydration doubles the apparent missing count. A diagnostic implementation needs enough phase information to distinguish absence from prior consumption.

## Keep the payload winner unchanged

The first hydrated payload wins for a duplicate key in the current helper. Instrumentation must not accidentally replace TryAdd with indexer assignment while collecting statistics, because assignment would make the last payload win. A counter can increment when TryAdd returns false while leaving the retained value unchanged. The test should assert the winning payload, not only the final identity sequence.

Likewise, counting unrequested hydrated keys should not cause them to enter the returned list. Diagnostic reporting and result construction are separate outputs. A convenient implementation that returns all dictionary values after iteration would change ordering and inclusion. The metrics may describe extras, but the API result must still exclude them under the existing contract.

Avoid enumerating a general input enumerable repeatedly merely to obtain counts. The helper currently consumes entities in one dictionary-building pass. Adding Count, GroupBy, and ToDictionary as independent operations can trigger multiple enumeration, additional work, or different behavior for a stateful source. A proposed implementation can update counters during the existing pass and maintain only the extra sets required by the chosen definitions.

## Worked diagnostic ledger

Use a table with phases rather than one final number. During hydration, A-first inserts A; A-second increments a duplicate-payload counter; C inserts C; X inserts X. During request traversal, the first A emits, B is absent, the second A is already emitted, and C emits. At the end, X is an unrequested hydrated key. This ledger explains every counter without guessing a deletion or permission failure.

Now change the fixture so A appears three times in the requested sequence. Requested occurrences increase, but distinct requested keys, missing distinct keys, and emitted entities remain unchanged. This mutation is an effective test of metric definitions. If a missing counter rises with repeated A, it is probably counting failed consumption rather than missing hydration.

Change the hydration fixture instead by adding a third A payload. Duplicate hydrated occurrences increase, while the selected payload remains A-first. Add another unrequested X payload: distinct extras remain one but duplicate hydrated occurrences increase. These small mutations separate occurrence metrics from distinct-key metrics and make dashboard interpretations more reliable.

## Totals and ratios require denominators

A hydration availability ratio could divide available distinct requested keys by distinct requested keys. That is different from emitted entities divided by requested occurrences when the request repeats identities. Choose the denominator according to the operational question. If the question is how many candidate identities could be materialized, repeated occurrences should not silently reduce the ratio as though they were unavailable entities.

Handle an empty requested set explicitly. A zero denominator should produce a defined absence of ratio or a documented neutral value, not an exception or a misleading perfect score. An empty search page also takes the controller's early-return path and does not call hydration or mapping. Instrumentation at the helper alone therefore cannot count every search request. Request-level metrics belong at a broader boundary.

Do not equate the response Total with the number of identities examined by this helper. Total describes the search result count reported by the service, while the helper receives one page's keys. A metric comparing emitted count directly with Total will look artificially low for ordinary paginated searches. A diagnostic should compare like scopes before alerting on a percentage.

## Avoid unnecessary identifiers in telemetry

Most operational questions can begin with counts, timings, and a request correlation identifier rather than template names, full payloads, or raw query text. Names and queries may contain information unnecessary for monitoring selection behavior. High-cardinality identity labels can also make metric storage expensive. A design should justify every field it emits instead of logging the entire request and hydration response by default.

For a controlled learning fixture, synthetic keys are enough to explain a mismatch. In an operational system, a bounded diagnostic sample may be useful under an approved data-handling policy, but that is a separate design decision. Hashing an identifier does not automatically make it anonymous or low-cardinality. A stable hash can still correlate the same entity across events and can still create one label value per entity.

Correlation should connect stages of one request without pretending to identify one immutable database snapshot. A request identifier says that search, hydration, and mapping belong to one execution. It does not prove that their data came from the same moment. This distinction preserves the consistency lesson while making execution easier to investigate.

## Measure latency at useful boundaries

A proposed trace can time search, hydration, selection, and mapping separately. The values answer different questions: storage access, payload retrieval, in-memory reconciliation, and representation work. The sum may not perfectly equal end-to-end HTTP duration because the host has additional work and measurement overhead. Document the boundaries rather than treating every discrepancy as a bug.

The Guid template-service overload returns Task.FromResult around repository results, so an asynchronous-looking signature does not itself establish asynchronous I/O. A timing label should name the stage rather than claim time spent awaiting a remote service. If an enumerable defers work, deciding where enumeration happens also affects which stage appears expensive. Inspect implementation and measurement placement before assigning a cause.

Diagnostics should fail according to a stated policy. A metrics sink outage should not normally change the selected item sequence, but blocking or throwing telemetry can do exactly that if placed carelessly. A disposable fault-injection exercise can make a diagnostic sink throw and test the proposed behavior. Do not claim that the current helper already isolates telemetry failures; there is no such sink in the inspected implementation.

## Exercise UM-18A: define a metric dictionary

Write definitions and expected values for the worked A, B, A, C fixture. Include both occurrence and distinct-key counters. Add a repeated requested A and a duplicate unrequested X payload, then update the values. Explain why a failed dictionary removal is not always missing hydration.

## Exercise UM-18B: protect behavior while instrumenting

Design a disposable instrumented selector that returns the same selected payload references as the current helper plus a separate diagnostic record. Specify tests for first-payload wins, repeated requested keys, extras, and one-shot enumeration. Include a diagnostic-sink failure policy. Do not modify the application helper for this exercise.

## Exercise UM-18C: review an operational dashboard

A dashboard divides returned item count by response Total and labels the result hydration success. Explain the scope mismatch using a page of five candidates from a hundred matches. Propose a better denominator, empty-input behavior, and two low-cardinality dimensions. Identify information that should not be logged merely to calculate the ratio.

## Review checkpoint

Useful diagnostics explain an observation without redefining the API. A strong design preserves first-payload selection and one-time emission, distinguishes absent keys from consumed keys, and compares quantities from the same scope. It treats request correlation, content privacy, and measurement overhead as explicit design decisions rather than incidental logging details.
