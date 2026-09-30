# 09. Controller tests and collaborator contracts

Read [SearchTemplateItemControllerTests](../../../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs). The suite constructs the real controller with mocked search, hydration and mapping collaborators. It supplies deliberately inconsistent sequences and inspects the returned PagedModel. This is a focused composition test: it establishes how the controller connects its collaborators under controlled inputs.

## Choose fixtures that reject plausible mistakes

The ordinary order case uses search A, B, C and hydration C, A, B. If both sequences were already sorted identically, a controller that skipped reconciliation could pass. The missing/extra case includes a total different from surviving item count. The duplicate case gives later payloads different aliases. Each fixture is designed to expose a specific wrong implementation.

For every test you add, write the mutation it should catch. Returning hydration directly should fail order and extra-entity cases. Replacing Total with response length should fail metadata cases. Dictionary assignment should fail first-payload assertions. Repeated requested lookup without consumption should fail duplicate-request uniqueness. This turns a test list into an explanation of coverage.

## Mock setup is not assertion by itself

A setup using It.IsAny accepts broad arguments. It does not prove that the controller forwarded the intended query, skip, take or exact key array. If argument forwarding is the claim, verify those arguments or use a matching setup that would fail for incorrect values. Keep the added assertion focused on externally meaningful collaboration rather than every incidental call detail.

For hydration, the expected key array includes the search sequence as extracted, including duplicates before the helper collapses output. A test that assumes the controller deduplicates keys before calling the service would encode a different current behavior. Read the key extraction before writing the mock expectation.

## Negative call assertions can matter

The empty-page regression explicitly verifies hydration and mapping are never called. That is stronger than checking an empty response alone, because an implementation could do unnecessary or dangerous broad work and still return no items. The nonempty-all-missing case should not inherit that assertion: it reaches hydration and mapping under the current ordinary path.

Do not use call counts as a substitute for values. One mapper call with the wrong order is still wrong. Combine collaborator observations with response identity, payload and metadata assertions. The right balance depends on the contract: skip unnecessary hydration is a call-level requirement, while preserve first payload is a value-level requirement.

## Exercise UM-09A: add an argument-forwarding case

Choose a distinctive query and nondefault aligned paging values. Configure search to return a controlled page and verify the exact object type, query, skip and take received. Then verify hydration receives the extracted key sequence. Keep the mapper simple enough that its output exposes the selected order.

Explain what this test does not establish. Direct controller invocation bypasses HTTP route matching, model binding, filters and authorization middleware. Passing the arguments directly proves method-level forwarding, not that a particular URL binds correctly in a running application. A later integration test can cover that boundary.

## Exercise UM-09B: test a dependency failure

Make hydration fail after a nonempty search result. Assert that mapping does not run and the action does not return a successful completed page. Choose a controlled exception or faulted task and document the narrow claim. Do not invent the final HTTP status unless the test includes the application's error-handling pipeline.

Then make mapping fail after successful hydration. Compare which stages completed. This helps distinguish a selection bug from a response-projection failure during debugging. Your report should name the failing collaborator and the observations available before the failure.

## Exercise UM-09C: review a too-broad assertion

A test asserts only that the result is OkObjectResult. List at least three incorrect outputs that would still pass: wrong order, extra entity and changed total are examples. Strengthen the test with an independent expected response. Keep type/status assertions where useful, but do not let them stand in for the collection contract.

## Review standard

A strong controller test uses adversarial but valid collaborator results, verifies the meaningful forwarded arguments, and checks response values and relevant skipped calls. Its report states that it is a direct controller test with mocks and does not inflate that into a full HTTP or database integration result.
