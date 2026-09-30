# 13. HTTP policy and inheritance evidence

The ordering helper can be correct while a claim about the endpoint's access rules is wrong. This chapter studies how to move from a direct controller test to a defensible HTTP claim. Its prerequisite is the distinction between an action method, the framework pipeline that invokes it, and the inherited metadata that participates in that pipeline. The outcome is an evidence map, not a new authorization implementation.

Start with [SearchTemplateItemController](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs). Its immediate base is [TemplateItemControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/TemplateItemControllerBase.cs), which derives from [ManagementApiControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs). That last class declares BackOfficeAccess and UmbracoFeatureEnabled authorization policies. The item base contributes route and API explorer metadata. These are concrete observations of the actual inheritance path.

## Similar names are not inherited behavior

There is another class named [TemplateControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/TemplateControllerBase.cs). It declares the TreeAccessTemplates policy and provides template operation status handling. SearchTemplateItemController does not derive from that class. A review that searches for the word template, finds this policy, and assigns it to every template endpoint has crossed a boundary without evidence.

The mistake is understandable because folder structure and domain vocabulary help navigation. They do not determine inheritance. Write the actual chain on paper before reading a nearby sibling. A node in that chain establishes a possible source of inherited metadata; a similarly named node outside it establishes nothing about this action until another mechanism connects them. A global convention could still add policy, but its existence would need separate evidence.

This reasoning also prevents the opposite mistake. Absence of TreeAccessTemplates on the chain is not proof that the endpoint is anonymous. ManagementApiControllerBase explicitly supplies other policies. Nor is it proof that no resource restriction exists anywhere else. The precise conclusion is narrower: the inspected inheritance path does not obtain TreeAccessTemplates from TemplateControllerBase. That statement remains useful without pretending the entire application has been audited.

## Direct invocation bypasses an important boundary

Imagine a unit test that constructs SearchTemplateItemController with three mocks and calls Search. It supplies no authenticated principal. Search returns an OkObjectResult containing ordered items. The test has established behavior of the method and collaborators under those inputs. It has not demonstrated that an unauthenticated network request can reach the method. Framework authorization normally runs outside this direct invocation.

The same distinction applies to version selection, route matching, model binding, filters, named JSON options, and exception translation. The base controller declares several relevant attributes. A direct method call is valuable precisely because it isolates orchestration, but it cannot silently inherit evidence about pipeline components it never executed. A result object with status information is also different from serialized bytes sent over an HTTP connection.

Build an evidence ladder with four rungs. Source inspection establishes declared metadata and control flow. A reflection or application-model test can establish how metadata is discovered under a particular setup. A hosted integration test can exercise the configured pipeline with a controlled principal. A deployed smoke test can add environment-specific configuration evidence. Higher rungs are not automatically better for every question: selection order is easier to diagnose at the unit level, while access control needs the layer that actually enforces it.

## Worked request trace

Use a hypothetical hosted test, clearly labeled as a design rather than an executed result. The request targets the registered versioned item-template search route with a text query. Routing selects an endpoint. Authentication constructs or fails to construct a principal. Authorization evaluates the policies attached to that endpoint under the host's registrations. Only an admitted request reaches Search, where search identities are retrieved, hydration occurs, selection runs, and mapping creates response items.

Draw two paths through this trace. On the rejected path, your useful assertion is not merely an error status; it is also that sensitive search and hydration work did not execute. On the admitted path, the useful assertions include the selected ordered identities and the response contract. If a hosted test replaces authorization with an unconditional allow handler, it cannot serve as proof of production policy behavior even though it uses an HTTP client.

Use three independent principal scenarios in the design: unauthenticated, authenticated but not satisfying an applicable policy, and satisfying all applicable policies. Do not assign exact expected statuses before confirming how this application's authentication and challenge configuration handles them. The test plan can state which outcome category is required and mark the concrete challenge or forbid response as pending configuration inspection. This is more honest than guessing a familiar 401 or 403 convention.

## Response content is a separate permission question

Admission to an endpoint and visibility of individual entities are different concerns. The controller forwards the Template object type, query, skip, and take to search. It then asks the template service for the resulting keys. The action body does not itself contain a per-item authorization predicate. That observation is not a complete claim about downstream services or all application policies; it identifies where a proposed per-item rule would need investigation.

Suppose a proposed extension hides certain templates from a caller. Filtering after search would create sparse pages and raise questions about Total. Filtering before search could change which identities are paged. Filtering only during mapping could leave metadata inconsistent or expose fields inadvertently. The access policy must define its relationship to selection and pagination before code placement is chosen. A secure implementation is not obtained simply by adding a Where clause at a convenient stage.

Totals themselves can communicate information. If a restricted response contains no items but reports the unrestricted matching count, the product must decide whether that count is permitted. This is a proposed design review concern, not an assertion that the current endpoint exposes unauthorized information. Good reviews distinguish an observable implementation fact from a threat hypothesis requiring a known policy and a reproducing request.

## A bounded investigation without a running site

Create an evidence worksheet with columns for claim, source, execution layer, and unresolved dependency. Record the actual base chain and policy names first. Then record what the existing direct controller tests establish. Leave HTTP status, policy handler behavior, and deployment configuration unresolved unless you actually inspect or execute those paths. The worksheet is useful even when a complete host cannot be built in the available environment.

For a safe learning exercise, use synthetic principals and synthetic template identities in a disposable test project. Never need real back-office credentials or production content to test the distinction between admitted and rejected requests. If obtaining a fully configured host requires package restore or a database, document that requirement before running anything. A source review that remains honest about its boundary is preferable to relabeling a pure unit test as an integration test.

When reporting a failure, include the test host configuration and any policy replacements. A test that swaps both authentication and authorization has a different purpose from one that only supplies a principal through a test authentication scheme while retaining real authorization handlers. Readers need this distinction to decide what the result supports. The name of the test framework does not convey it automatically.

## Exercise UM-13A: reconstruct the authority chain

Produce a diagram from SearchTemplateItemController through its actual bases. Attach route, authorization, and response-related metadata only to the class that declares it. Place TemplateControllerBase beside the chain and explain why its TreeAccessTemplates declaration cannot be attributed through inheritance. Identify one additional mechanism that could affect effective policy and the evidence needed to inspect it.

## Exercise UM-13B: design two tests with different claims

Write specifications for a direct action test and a hosted authorization test. The direct test should verify an ordered sparse result. The hosted test should verify that a rejected request cannot invoke the search collaborator. List the configuration that must be retained for the hosted result to support the intended claim. State explicitly what neither test proves about a deployed installation.

## Exercise UM-13C: review a proposed visibility rule

A product request says that some template items should be hidden from a particular class of user. Compare filtering before paging, after hydration, and during response mapping. Define what Total means in each proposal and what the client may infer from it. Recommend a contract before recommending a code location. Your answer must distinguish a proposed policy from the inspected current action.

## Review checkpoint

A defensible submission names the real inheritance chain, preserves the distinction between metadata and enforcement, and limits every conclusion to its evidence. It neither invents anonymous access nor imports a sibling controller's policy. Its test plan can fail for an access violation independently of whether the ordering algorithm is correct.
