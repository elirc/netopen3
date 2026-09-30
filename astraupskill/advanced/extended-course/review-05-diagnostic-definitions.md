# Review guide: diagnostic definitions and behavior preservation

Attempt chapter 18 before reading the worked answers. A useful first hint is to stop using missing as a synonym for failed Remove. Write down whether the key never appeared in hydration or appeared and was already emitted. The same dictionary operation can have different explanatory meanings depending on earlier state.

## UM-18A: occurrence and identity are separate dimensions

For requested A, B, A, C and hydrated A-first, A-second, C, X, requested occurrences are four and distinct requested keys are three. Hydrated occurrences are four and distinct hydrated keys are three. Duplicate hydrated occurrences are one. The selected sequence contains two entities. One distinct requested key, B, lacks hydration; one distinct hydrated key, X, was not requested.

Adding a third requested A raises requested occurrences to five and repeated-request occurrences to two, while the distinct and emitted quantities stay unchanged. Adding another hydrated X raises hydrated occurrences to five and duplicate hydrated occurrences to two, but distinct extras remain one. This is why every metric should state whether it counts occurrences or identities.

The second and third A removal attempts fail because the entry was consumed, not because A was absent from hydration. An implementation that increments missing hydration for every failed removal reports the wrong phenomenon. It may still be useful to count repeated requested occurrences, but that metric needs its own name and definition.

Full credit requires exact values for each fixture and a reason for every change. Partial credit applies when the values are correct but the names remain ambiguous. An answer that uses deleted for missing should be revised: deletion is one possible cause, not an observation established by this helper.

## UM-18B: instrumentation should preserve selected references

A disposable instrumented selector can retain the original TryAdd and Remove logic while collecting additional sets or counters. It should never replace the stored payload when TryAdd fails. The selected references must match uninstrumented behavior exactly, including A-first rather than A-second in the duplicate fixture.

Use a single-use enumerable that throws if enumerated twice to detect an instrumentation design that performs Count and then loops again. A successful first pass followed by a second diagnostic pass is still a behavior change for that input contract. The model can collect hydration occurrence count and duplicate count inside the initial loop rather than re-enumerating.

A separate diagnostic record can describe distinct missing and extra keys without exposing their payloads. If a sink is part of the prototype, define whether it is best-effort or part of the request contract. For a best-effort policy, a controlled sink failure should leave the selected output unchanged and should follow an explicit fallback reporting rule. Swallowing every exception from arbitrary application work is not an acceptable substitute for isolating the diagnostic boundary.

The exercise is complete when tests compare both identity order and winning payload references, include repeated requests and unrequested extras, and establish the enumeration count. It remains a disposable prototype until integrated into the actual helper with appropriate review. A model passing its own tests does not prove that the production implementation has been instrumented correctly.

## UM-18C: compare one page with one page

A response may report a hundred matching candidates while the request examines only five. Returning all five available entities would produce five percent under the proposed dashboard formula, even though hydration was complete for the examined page. The denominator has mixed the global search scope with the page scope.

A better page-level availability ratio compares available distinct requested keys with distinct requested keys on that page. If duplicates are meaningful for another operational question, record their count separately. An empty requested set should produce a documented no-sample result or another explicitly defined value; it should not trigger division by zero or silently claim a perfect observation.

Low-cardinality dimensions might include endpoint name and outcome category, provided both are bounded sets. Raw template identity, query text, and template name should not become metric labels just because they are available. The count calculation does not require those values. A request correlation identifier can help connect logs, but it should not be used as an unbounded metric dimension.

The review should also reject an alert that labels every sparse page a service failure. Under the current contract, sparse reconciliation can be an expected result. A useful alert needs a baseline, a sustained condition, and a clearly defined operational question. This chapter designs measurements; it does not establish production thresholds or claim that a particular missing rate is acceptable for every installation.
