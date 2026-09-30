# 11. Shared helpers and caller semantics

OrderByRequestedIds lives on a shared management controller base and has several callers. Inspect [SearchDataTypeItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/DataType/Item/SearchDataTypeItemController.cs) and [BatchDocumentTypesController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/DocumentType/BatchDocumentTypesController.cs) beside the template search action. They reuse selection/order mechanics but do not necessarily share response metadata semantics.

## Shared mechanism, different total

The search controller preserves Total from its search result. The batch document-type controller computes Total from the response model count. These are different public meanings. A refactor that extracts response construction along with the ordering helper could accidentally impose one total policy on both callers. Keep the generic helper responsible for what its inputs can establish: selected ordered entities.

This is an example of a useful abstraction boundary. The helper receives entities and requested identities, not search metadata, page position or endpoint intent. It can enforce membership, uniqueness, order and payload selection. It cannot decide whether an endpoint's total should mean all search matches or surviving batch items. That decision belongs to the caller contract.

## Collection type affects ordering claims

The batch endpoint accepts a HashSet of identifiers and converts it to an array before calling the helper. The helper preserves the order of that supplied array. This does not automatically establish a universal wire-order guarantee for repeated query parameters through model binding and set construction. If an endpoint promises request order externally, its input representation and binding behavior need appropriate evidence.

The template search path is clearer about its immediate ordering authority because keys come from an ordered search enumeration materialized into an array. Do not transfer that exact explanation to every caller merely because the same helper is used. A shared function can provide a narrower guarantee than a public endpoint might appear to imply.

## Search sibling controllers reveal a family pattern

The data-type search controller follows the same search, empty check, key extraction, hydration, ordering, mapping and total-preservation pattern. This can help you identify regression risk across similar endpoints. But a copied test suite should still use the correct service and model types and verify any caller-specific behavior. Mechanical duplication without understanding can reproduce the same blind spot everywhere.

When changing a shared helper, inventory callers first. Separate those that depend on first-payload tolerance from those whose services normally guarantee unique entities. Even if duplicates are unusual, changing tolerance to rejection can be a breaking behavior. The safe review question is which callers' observable contracts change under the proposed modification.

## Exercise UM-11A: make a caller matrix

For template search, data-type search and document-type batch, record requested-ID source, hydration service, empty-input behavior, response model and Total authority. Link each claim to the actual source. Add an unknown column for behavior not established by direct inspection, such as HTTP binding order.

This matrix should reveal shared mechanics without erasing differences. A generic label such as all batch endpoints behave the same is not enough. The goal is to guide a focused regression plan for a shared-helper change.

## Exercise UM-11B: review a generic response builder

Someone proposes a helper that orders entities, maps them and sets Total to mapped count for every caller. Write a concrete template-search counterexample where searchTotal is nineteen and survivors are two. Explain how the proposed abstraction corrupts metadata despite returning correct item order.

Offer a narrower extraction or explicit total parameter if an abstraction is justified. The design should make the authority visible rather than hide it behind a convenient default. Consider whether the existing small repeated response construction is clearer than a generalized builder with policy flags.

## Exercise UM-11C: assess a duplicate-policy change

Propose changing first-wins to reject-duplicates. Identify which shared callers need review and which tests would demonstrate the new failure behavior. Explain how a previously tolerated service anomaly could now make an entire endpoint fail. The proposal may still be reasonable under a new contract, but it is not a behavior-preserving cleanup.

## Review standard

A strong answer inventories actual callers, preserves endpoint-specific metadata and avoids overstating ordering guarantees from a set-shaped input. It treats shared-helper changes as shared behavior changes and chooses abstractions that keep policy authority visible.
