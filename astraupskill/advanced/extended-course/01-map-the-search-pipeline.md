# 01. Map the search pipeline

A large repository becomes manageable when you follow one behavior across a few boundaries. Begin with [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs). The action asks an entity-search service for a page of slim entities, extracts their keys, asks a template service for full templates, orders the hydrated templates by the requested keys, maps them to item responses and preserves the search total.

## Name what each stage knows

The search result knows the page's ordered identities and total count under its search contract. Slim entities are not the full templates needed by the response mapping. The template service returns hydrated models for keys, but the controller does not assume that enumeration order matches the search order. The ordering helper reconciles those two representations. The mapper then transforms selected template models into the public response shape.

This is a small integration pipeline, not one opaque query. Different stages can disagree because of ordering, missing entities, duplicate payloads or extra returned models. A useful source map writes the value crossing each arrow: PagedModel of slim entities, Guid array, enumerable of templates, ordered template list, mapped item list. Labels such as data and results are too vague to support debugging.

## Read the actual search implementation

The injected interface used here is [IEntitySearchService](../../../Umbraco-CMS/src/Umbraco.Core/Services/IEntitySearchService.cs), whose documented purpose is direct database entity search. The local [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs) converts skip/take to paging, constructs a filter, obtains paged descendants ordered by node text and materializes them to an array. Do not automatically describe this path as a relevance-ranked external search index because another search service exists elsewhere in the CMS.

The controller's ordering contract is still valuable regardless of how the search order was produced. It preserves the order supplied by this stage rather than replacing it with service hydration order or a new alias sort. If a future search implementation changes its ranking, the controller should continue respecting the declared ordered identity sequence unless the API contract is deliberately changed.

## Follow the hydration boundary

The Guid-key overload in [TemplateService](../../../Umbraco-CMS/src/Umbraco.Core/Services/TemplateService.cs) delegates to repository GetMany and returns the resulting templates. Another overload accepting aliases orders by name, but it is not the overload called by this controller. Overload identity is part of source tracing. Reading a nearby method with a similar name can lead to a plausible but incorrect explanation of the actual path.

The controller explicitly invokes OrderByRequestedIds after hydration. That is evidence that the response order is enforced at the integration point. A test can supply scrambled hydration order and verify the mapper receives the requested sequence. It does not need a full CMS installation to establish that controller composition, although broader integration behavior requires other layers of testing.

## Exercise UM-01A: draw the five-stage map

Draw search, key extraction, hydration, reconciliation and mapping. For each stage, list input type, output type, ordering authority, possible missing information and side effects you can establish from the inspected source. Distinguish a synchronous service call from an awaited method without assuming that every Task-returning method performs remote asynchronous I/O.

Add an empty-search branch to the diagram. The controller returns a paged response with the search total and does not call hydration or mapping when the search page has no items. This is a separate path, not merely the ordinary path with an empty final list. The difference is observable through service-call assertions.

## Exercise UM-01B: trace one template payload

Use synthetic keys A, B and C. Search returns B then A; hydration returns A, C and B. Predict which templates reach the mapper and in what order. Give each template a distinctive alias so you can observe payload identity as well as key order. Then explain why C's exclusion is selection behavior, not just sorting.

Keep search Total distinct from the number of returned items. The total can describe a broader result set across pages. A two-item mapped response does not justify rewriting it to two. Later chapters investigate the metadata contract in more detail.

## Exercise UM-01C: audit your own claims

Write three source-backed claims and three questions the narrow trace does not answer. Good unanswered questions include HTTP model-binding behavior, full authorization execution, repository consistency across calls, or browser display of sparse pages. Do not fill those gaps with assumptions. The purpose is to know exactly how far the evidence reaches.

## Review standard

A strong source map follows the actual injected interface and overload, identifies where order becomes authoritative, and separates the empty branch. It should let another learner locate a failure stage without reading the entire CMS. The final two-minute explanation should describe why hydration order is not allowed to replace search order.
