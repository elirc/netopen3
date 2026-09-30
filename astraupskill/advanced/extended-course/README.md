# Umbraco search-integration workbook

This extended course studies a narrow real path through a large CMS: template search produces ordered identities, a service hydrates entities, a shared helper selects and orders them, and a mapper produces response models. The path teaches collection contracts, data reconciliation, pagination, tests, asynchronous boundaries, API review and maintenance.

Use the [earlier advanced route](../README.md) for a shorter introduction. Work with source-linked examples and synthetic template identities. Proposed changes to diagnostics, paging or cancellation remain exercises until implemented and verified separately. Preserve existing application code and learner answers while experimenting in a disposable harness or branch.

The completed route contains 23 chapters, 69 independent exercises, six separate worked review guides, and two complete instructional capstone briefs. It contains more than 37,000 instructional prose words excluding code and navigation. The capstones propose application extensions; their implementation remains learner work.

## Progressive route

Read chapters 01-04 for the pipeline and contract, 05-08 for payloads and execution, 09-12 for tests and input meaning, 13-18 for integration proposals, and 19-23 for reproducible evidence, labs, capstones, and assessment. Attempt exercises before opening the matching review guide.

- [01. Map the search pipeline](01-map-the-search-pipeline.md)
- [02. Write an ordering contract](02-write-an-ordering-contract.md)
- [03. Dictionaries and consumption](03-dictionaries-and-consumption.md)
- [04. Pagination metadata and sparse pages](04-pagination-metadata-and-sparse-pages.md)
- [05. Payload identity and duplicate policy](05-payload-identity-and-duplicate-policy.md)
- [06. Enumerables and materialization](06-enumerables-and-materialization.md)
- [07. Async signatures and cancellation claims](07-async-signatures-and-cancellation-claims.md)
- [08. Mapping and response identity](08-mapping-and-response-identity.md)
- [09. Controller tests and collaborator contracts](09-controller-tests-and-collaborator-contracts.md)
- [10. Independent oracles and properties](10-independent-oracles-and-properties.md)
- [11. Shared helpers and caller semantics](11-shared-helpers-and-caller-semantics.md)
- [12. Search and paging input semantics](12-search-and-paging-input-semantics.md)
- [13. HTTP policy and inheritance evidence](13-http-policy-and-inheritance-evidence.md)
- [14. Consistency between search and hydration](14-consistency-between-search-and-hydration.md)
- [15. Bounded refill as an explicit proposal](15-bounded-refill-as-an-explicit-proposal.md)
- [16. Cursors and stable ordering](16-cursors-and-stable-ordering.md)
- [17. Client state and response arrival](17-client-state-and-response-arrival.md)
- [18. Diagnostics without changing the contract](18-diagnostics-without-changing-the-contract.md)
- [19. Build boundaries and reproducible evidence](19-build-boundaries-and-reproducible-evidence.md)
- [20. An isolated source lab and an integration ladder](20-isolated-source-lab-and-integration-ladder.md)
- [21. Capstone: observable reconciliation without semantic drift](21-capstone-observable-reconciliation.md)
- [22. Capstone: a continuation contract across server and client](22-capstone-continuation-and-client-contract.md)
- [23. Assessment, maintenance, and handoff](23-assessment-maintenance-and-handoff.md)

## Separate hints, solutions, and rubrics

- [Review guide: search, selection and payload identity](review-01-search-and-selection.md)
- [Review guide: enumeration, completion and mapping boundaries](review-02-enumeration-async-and-mapping.md)
- [Review guide: test strength, shared callers and input meaning](review-03-testing-callers-and-inputs.md)
- [Review guide: HTTP evidence, consistency, continuation, and client state](review-04-http-consistency-and-client-state.md)
- [Review guide: diagnostic definitions and behavior preservation](review-05-diagnostic-definitions.md)
- [Review guide: build evidence, isolated labs, and capstone assessment](review-06-build-lab-capstones-and-assessment.md)

## Executable evidence

The [isolated selection lab](lab/README.md) extracts the current helper into a new external directory. Reference mode passed six checks. The signature-compatible starter compiled and failed only the deliberate duplicate-emission case; repairing the disposable copy restored six passes. Existing, relative, and in-source output paths were refused. The original helper remained unchanged.

This verifies the extracted method with a minimal entity contract. It does not execute the full controller, HTTP policies, mapper, database, or client UI. The existing focused controller project is described in chapter 19; no broad application suite was run for this completion pass. The organizer completion report records exact counts, file hashes, checks, and limitations.
