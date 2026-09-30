# 07. Async signatures and cancellation claims

The template search action is asynchronous because it awaits template hydration. It also accepts a CancellationToken parameter. Read the method body and the called [ITemplateService](../../../Umbraco-CMS/src/Umbraco.Core/Services/ITemplateService.cs) signature carefully. In this local action, the token is not passed to the synchronous search call or the Guid-key GetAllAsync call, and the body does not explicitly check it. A parameter name is not evidence that every operation honors cancellation.

## Await does not imply remote work

The local TemplateService Guid overload delegates to a repository and returns Task.FromResult of the templates. That is a Task-shaped service contract, but the inspected method does not itself contain an await or a cancellable remote operation. Do not describe the controller as performing a parallel distributed search pipeline merely because one method name ends in Async.

The abstraction can still be useful for callers and future implementations. The learning skill is precision: identify where work actually happens, where exceptions can be thrown, and which task is awaited. A synchronous exception before a Task is returned and a faulted awaited Task can reach the caller through different immediate execution paths, even if an async action ultimately reports failure for both.

## Cancellation has several possible meanings

A client disconnect, a canceled action parameter, and a service that stops its work are related but not identical events. The current narrow unit test can pass a canceled token and observe whether the action itself checks or propagates it. That does not prove how the full hosting pipeline handles an aborted request or whether lower-level database work has independent cancellation behavior.

If you propose cancellation support, define the desired boundary. Should the action stop before hydration when cancellation is already requested? Should a token flow through service interfaces and repositories? What happens if cancellation arrives after hydration but before mapping? Each choice needs a supported API path and tests. Adding ThrowIfCancellationRequested at one point is a partial policy, not automatic end-to-end cancellation.

## Exercise UM-07A: draw the execution sequence

Label synchronous search, empty-branch return, key extraction, service call, await continuation, ordering, mapping and response creation. Mark which steps can throw based on the source or a controlled mock. Then state what the action has completed if hydration faults. Mapping should not have produced a response merely because search succeeded.

Use call-observation assertions to distinguish stages. A mapper that is never invoked after hydration failure is a meaningful boundary check. Avoid asserting private implementation order unrelated to the contract, but preserve ordering where it prevents work or partial output after a failed dependency.

## Exercise UM-07B: investigate a canceled token

Pass an already-canceled token in a direct controller test with valid service stubs. Predict the current body behavior from its actual use of the parameter. Label the result as a direct-action observation. Do not claim it proves the full HTTP server ignores client disconnection, because that broader path includes hosting behavior outside this method invocation.

Then write a proposed cancellation acceptance matrix: canceled before search, canceled before hydration, canceled during supported hydration, and canceled before mapping. Identify which rows require changes to service signatures rather than only controller code. This prevents a superficial parameter pass from being reported as a complete feature.

## Exercise UM-07C: review Task.Run as a proposed fix

Someone suggests wrapping the synchronous entity search in Task.Run to make the endpoint async. Explain why moving work to another thread does not by itself make the repository operation cancellable or establish better server throughput. The proposal needs a workload and ownership analysis, not just an Async suffix.

For this learning exercise, compare the actual call contract and a hypothetical truly asynchronous service API. State what would need to change in interfaces, implementation, mocks and tests. Avoid version-specific framework recommendations without consulting the relevant official documentation for the installed environment.

## Review standard

A strong answer distinguishes asynchronous control flow, actual I/O, token propagation and hosting cancellation. It makes narrow claims from direct controller tests and labels broader cancellation support as a proposal until implemented. The final sequence diagram should show exactly which work is skipped after each controlled failure.
