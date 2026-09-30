# 08. Mapping and response identity

The ordering helper returns domain entities; the mapper creates TemplateItemResponseModel objects. Read the template mapping in [ItemTypeMapDefinition](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Mapping/Item/ItemTypeMapDefinition.cs) and the [response model](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/ViewModels/Template/Item/TemplateItemResponseModel.cs). The mapping copies Key to Id, uses an empty name when source Name is null, and copies Alias. Selection policy should already be resolved before this stage.

## Test the mapper boundary honestly

The controller regression suite uses a mapper mock that projects Id and Alias from the supplied templates. This makes ordered input visible in the returned response. It is a good controller-composition test, but it does not prove the real mapper's configuration or every response field. A separate mapping test would be needed for Name fallback and the registered real map.

Do not overstate a mock's evidence. If the mock preserves order, the controller test shows that the controller supplies the right ordered enumerable to that collaborator and returns its mapped output. It does not establish that an arbitrary future mapper implementation cannot reorder or filter. The mapper contract and real implementation require their own review where that claim matters.

## Identity should survive transformation

For selected template A1, the response Id should be A's Key and Alias should come from A1 under the first-payload policy. A mapper that invents new identifiers or looks up aliases independently can disconnect selection evidence from the output. Use distinctive identities and aliases so the test can detect swapped associations, not merely correct counts.

An array containing the expected set of IDs can still associate the wrong alias with each ID if a mapping bug zips unrelated sequences. Compare pairs or full expected records. The same principle appears in data migrations: preserving row count or identity set alone does not prove field associations remain correct.

## Missing names and required aliases

The real mapping supplies an empty string when Name is null and copies Alias. The response model declares Alias required at the C# model level. That declaration is not a general runtime validator for arbitrary JSON or a proof that every mock supplies meaningful text. In source-reading exercises, distinguish compiler construction requirements from the actual domain guarantees and serialization behavior.

A proposed response change, such as adding a diagnostic marker for missing hydration, belongs to the response contract. It should not be smuggled into Alias or Name because those fields already mean template data. Define new metadata explicitly and consider compatibility rather than overloading a display field with implementation status.

## Exercise UM-08A: create association fixtures

Use three templates with unique Key, Name and Alias combinations. Scramble hydration order and include a duplicate key with a different alias. Predict the exact ordered response tuples after selection and mapping. A test should fail if aliases are shifted one position even when all IDs remain present.

Add a null Name case for a real-mapper test if you construct the actual mapping configuration. If you use a simplified mock, state that Name fallback is outside its verified scope. Do not copy the production mapping into the expected-value function and then call the result independent evidence.

## Exercise UM-08B: distinguish empty paths

An empty search page skips mapping entirely. A nonempty search page whose hydrated keys are all missing reaches mapping with an empty selected enumerable. A mapper failure on that latter path can therefore behave differently from the early return. Write service-call expectations for both cases and explain why equal final item counts do not imply identical execution.

This is a useful negative-space test: it establishes when a collaborator must not be called and when it still belongs to the ordinary path. Avoid broad assertions that any empty response skips all services. The source short-circuits based on search Items, not the eventual selected count.

## Exercise UM-08C: propose mapping isolation

Design a focused test that uses the real template item mapping without booting unrelated CMS features, if the repository's mapping-test infrastructure supports it. Identify required registrations and fixtures from existing tests before inventing a new harness. Record whether it tests mapping alone or the full controller.

If setup cost is too large for the current learning exercise, provide the plan and retain the controller mock test's narrower claim. Honest scope is preferable to a copied mapper implementation presented as proof of the real configuration. The purpose is to know which boundary each test establishes.

## Review standard

A strong answer preserves identity-to-payload associations, distinguishes mock projection from real mapping, and tests the two empty paths separately. The final fixture table should catch correct-count but wrong-association results and explain which response fields are actually verified by each test layer.
