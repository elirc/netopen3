# 05. Payload identity and duplicate policy

Two objects can have the same entity key while carrying different fields. The ordering helper's duplicate policy therefore chooses more than a position; it chooses which payload represents that identity. Read the duplicate regression in [SearchTemplateItemControllerTests](../../../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs). Its aliases a-first and a-later make the winner observable.

## Separate key equality from object equality

The dictionary uses the entity Key as its lookup identity. It does not compare every field or use reference equality to decide whether two templates are duplicates. A1 and A2 can be different object instances with the same key. TryAdd retains A1 if it appears first, even if A2 has a different alias. The later object is not merged into the earlier one.

A test using two references to the same mock object cannot fully establish this policy because both winners look identical. Use distinct payload objects and distinctive fields. Assert both the output key and the chosen payload data, or reference identity in a direct helper test where that is the intended contract. The controller-level mapper can expose the distinction through Alias and Id.

## First means enumeration order

The first hydrated entity is the first one encountered while enumerating the supplied sequence. It is not necessarily the oldest database row, latest revision, alphabetically first alias or first requested occurrence. Requested order controls where a key appears; hydration order controls which duplicate payload is retained. These are two independent orderings with different jobs.

Construct hydration A-old, B, A-new and requests A, B. The current policy keeps A-old because of enumeration order, regardless of the labels old and new. If a product needs highest-version selection, it must receive and compare a version under a new explicit policy. Do not infer freshness from list position or from a friendly test label.

## Duplicate diagnostics are a proposal

The current helper tolerates duplicate payload keys by retaining one. A future diagnostic could record that duplicates were encountered without changing the response. Such a feature needs a definition of duplicate count: extra payload occurrences, number of keys with duplicates, or conflicting payloads only. Those are different metrics.

For example, A1, A2, A3, B1 contains two extra occurrences but one duplicated key. If A1 and A2 have identical visible fields, they are still duplicate identities under the helper, while a conflict metric might choose to count only differing payloads. Define the metric before adding logging, and avoid serializing full templates merely to count duplicates.

## Exercise UM-05A: build winner fixtures

Create three distinct objects for key A and two for B. Arrange them in an interleaved hydration sequence and request B before A. Predict winner and output position independently. Then permute only the later duplicate objects and explain why the result should remain unchanged while the first object for each key stays fixed.

Next move a later A object to the beginning. The output position for A remains determined by the requested sequence, but its selected payload changes under first-wins. This pair of experiments separates the two contracts more clearly than a single duplicate example.

## Exercise UM-05B: review last-wins code

A refactor replaces TryAdd with dictionary assignment. Write the smallest fixture that distinguishes the implementations. Include two payloads with the same key and different aliases, and one requested occurrence. Explain why ordinary unique-key tests all still pass. The regression must assert the payload winner, not just that duplicates collapse.

Then compare a reject-duplicates policy. It may be appropriate when duplicate payloads signal unacceptable corruption, but it changes the helper's current tolerance and affects shared callers. A proposal should identify error behavior, telemetry and migration of tests. It is not a neutral readability refactor.

## Exercise UM-05C: investigate mutable keys

As a robustness exercise, consider an entity whose Key changes after it is indexed. The dictionary key remains the value captured at insertion, while the object may later report another Key. Explain why stable identity during this operation is an important assumption. Do not claim the helper freezes objects or defensively copies all entity data; it returns selected references.

If you build a synthetic mutable fixture, label it as an assumption probe rather than evidence of an actual TemplateService bug. The current source path may provide stable models under ordinary use. The exercise teaches you to identify ownership assumptions without inventing a production failure.

## Review standard

A strong answer distinguishes lookup identity, payload instance, hydration order and requested order. It uses visibly different duplicate objects, catches a last-wins mutation, and states the stable-key assumption accurately. The final contract should make clear that first-wins is a deterministic selection policy, not a guarantee that the selected data is the newest possible version.
