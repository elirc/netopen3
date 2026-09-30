# Extend confidence without copying the worked case

## UMB-01 — a complete contract table

Create eight cases: all present, all missing, only extras, duplicate requested IDs, duplicate hydrated payloads, empty request, reversed hydration with unique IDs and mixed missing/extra/duplicate rows. Give every entity a distinct alias so payload selection is observable. Predict IDs, aliases, Total and whether hydration/mapping should run at the controller boundary.

Use the existing [regression source](../../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs) as an API reference after writing predictions. Do not count a helper-only exercise as evidence of controller short-circuit calls.

## UMB-02 — properties that survive many examples

Propose checks for: every output ID was requested and hydrated; no output ID repeats; output order follows first requested occurrence; first hydrated payload is selected; empty search avoids downstream work. Then write one condition under which a tempting property is false. For example, arbitrary hydration permutation can change selected aliases when duplicates differ.

Deliver a finite set of small exhaustive examples or a bounded generator with a fixed reproducible seed. When it fails, print a minimal counterexample. Avoid a property so weak that an implementation returning an empty list always satisfies it.

## UMB-03 — request order versus display sorting

A product request asks to alphabetize templates after hydration. Write a change proposal before coding. Which component owns the new ordering? What happens to search rank and paging? Can you sort only a page and honestly describe the whole result as alphabetized? State the changed API promise and choose tests that distinguish it from the existing requested-order policy.

This is a design exercise. Do not quietly insert an alphabetic sort into the current helper, whose other consumers may rely on requested order.

## UMB-04 — investigate missing identities

Design diagnostic counters for requested, hydrated, emitted, missing and extra identities. Separate distinct counts from row counts when duplicates exist. Use counts or approved non-sensitive identifiers instead of dumping whole entities. Explain how diagnostics could reveal index lag while leaving response behavior unchanged.

Acceptance: the counters reconcile for a worked mixed case, and the documentation says they indicate a discrepancy rather than proving its cause. Decide where sampling or bounded logging belongs before proposing production instrumentation.

## UMB-05 — review performance claims

Prepare unique and duplicate-heavy datasets with controlled sizes. Explain what input dimension each benchmark changes and what remains fixed. Warm-up, fixture construction and mapper/service work can distort a helper comparison; separate them if you measure the helper itself. Report time and allocation observations as measurements from that environment.

Your submission must not replace a behavioral assertion with a stopwatch threshold. A faster result that includes extras is still wrong. A unit test of ten entities does not establish production throughput.

[Compare your reasoning](05-solutions-and-rubric.md)
