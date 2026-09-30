# Capstone: defend a reconciliation contract

Prepare a small reviewable extension to the focused test suite or a separately labeled teaching harness. Your deliverable must show that you can transfer the identity-selection rule to a new fixture and explain its integration limits.

## Milestone 1 — define the contract before implementation

Write a paragraph specifying membership, first-request ordering, duplicate payload policy and metadata handling. Include one example where Items.Count differs from Total. Ask a reviewer to propose an ambiguous case and resolve it explicitly.

## Milestone 2 — challenge plausible bugs

Create at least four independent cases that would fail if the implementation: emitted extras at the end; used last hydrated payload; repeated requested identities; or replaced Total with returned count. For each, name the exact assertion that distinguishes the wrong behavior. Avoid adding dozens of near-identical rows without a reason.

## Milestone 3 — prove a property with limits

Check a bounded family of input arrangements and document the allowed identity/payload domain. Explain why an always-empty result would fail your property set. State when hydration permutation is allowed to change payloads. The goal is to discover and explain counterexamples, not to report a large case count without interpretation.

## Milestone 4 — reproduce actual-source evidence

Run the focused project in a suitable environment and retain its command and test summary. If setup prevents execution, provide the completed teaching-harness result separately and mark the actual-source test as not run. Do not substitute the harness's success for an upstream compilation or controller test claim.

## Milestone 5 — handoff

Submit the contract, minimal fixtures, source links, checks, observed results and a short troubleshooting note. Include a paragraph explaining one concern the helper does not solve: search-index freshness, global sorting before pagination, database consistency, authorization or production performance.

A reviewer should be able to change one fixture and predict the outcome using your explanation. If the explanation only works for the original A/B example, add a new domain scenario such as ranked product IDs hydrated from a catalog. Keep the same explicit policy or clearly state where the new domain requires a different one.
