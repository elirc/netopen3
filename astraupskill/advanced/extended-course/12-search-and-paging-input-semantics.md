# 12. Search and paging input semantics

The concrete [EntitySearchService](../../../Umbraco-CMS/src/Umbraco.Infrastructure/Services/Implement/EntitySearchService.cs) has distinct query branches. Whitespace-only input produces an unrestricted query of the requested object type. A query parseable as a GUID matches entity Key. Other text uses a name Contains predicate. The template controller supplies the Template object type. These facts are more precise than a generic statement that the endpoint searches everything.

## Query shape changes meaning

A GUID-looking string is interpreted as identity rather than a name substring under this implementation. If a template's name happens to contain such text, the GUID branch still follows its stated key predicate. A test should make the two interpretations produce different results so the branch is observable. Do not assume tokenization, fuzzy matching or relevance scoring from the word search alone.

The source expresses a name Contains query through the repository query abstraction. Exact database collation and case behavior should be investigated at the relevant integration layer before making a universal claim. A unit test of query construction and a real database query can establish different aspects. Keep those scopes separate in the report.

## Skip/take conversion has a narrow rule

Read [PaginationHelper](../../../Umbraco-CMS/src/Umbraco.Core/PaginationHelper.cs). It requires skip to be a multiple of take, then assigns page size to take and page number to integer division skip/take. Its documentation describes zero-based page number. The helper does not contain a complete explicit validation policy for every negative or zero input.

For take zero, the modulo expression itself is problematic rather than producing the helper's named nonmultiple ArgumentException branch. Negative values can also behave according to arithmetic unless another layer rejects them. Do not infer the final HTTP response from this helper alone. The purpose of the exercise is to locate validation ownership and identify unproven assumptions, not to declare that the public endpoint accepts every arithmetic input.

## Controller defaults are part of the method contract

The search action defaults skip to zero and take to one hundred. Direct method calls that omit those arguments use the declared defaults. HTTP binding and validation may involve additional framework or application components. A direct controller test can verify forwarding of explicit nondefault values, but it cannot alone prove the entire URL parsing contract.

The base controller contains a SkipTakeToPagingProblem helper with a specific bad-request explanation, but its existence does not prove this action calls it. Follow actual control flow before citing a status response. A nearby helper is a clue to investigate, not evidence of invocation.

## Exercise UM-12A: build a query-meaning table

Use empty, whitespace-only, ordinary text, a valid GUID string and text that resembles but does not parse as a GUID. Predict the predicate branch from the implementation. Construct a fixture where identity and name matching differ. Label any case-sensitive matching claim as requiring the actual database/query provider evidence if you have not run it.

For the template route, include another entity type with a matching name and explain why the supplied object type matters. A query test that ignores object type can return plausible results while violating endpoint scope. Verify both the query and type forwarded by the controller where that is the intended claim.

## Exercise UM-12B: test arithmetic boundaries

For the pure helper, examine skip zero/take five, skip ten/take five, skip seven/take five, take zero and selected negative values. Record actual helper results or exceptions in a disposable test. Then separately trace the HTTP path to determine which inputs can reach it and how errors are translated.

Do not write a test named returns HTTP 400 if you only invoke the arithmetic helper. The observed exception and the public response are different layers. A complete improvement proposal can add clear validation and map errors, but it must preserve compatible valid paging behavior and receive its own tests.

## Exercise UM-12C: propose a clearer input contract

Define accepted skip/take bounds, alignment requirements and behavior for zero or negative values. Decide whether arbitrary offsets should be supported or rejected under the existing page-number service contract. Explain the compatibility impact of changing alignment policy. An apparently friendly acceptance of arbitrary skip can require a different underlying query strategy.

Write acceptance examples before implementation and identify the authoritative validation point. Client controls can guide users but do not replace server-side checks. The proposal should produce predictable errors without claiming that every existing caller already follows the new rule.

## Review standard

A strong answer derives query branches and paging arithmetic from the actual implementation, avoids invented search capabilities, and separates helper behavior from HTTP handling. It identifies where a validation policy is missing or unproven and proposes a precise extension instead of guessing from nearby code.
