# Use the focused controller suite as the boundary

The acceptance command uses [TemplateOrder.Tests.csproj](../scripts/template-order-tests/TemplateOrder.Tests.csproj). This focused project links the actual SearchTemplateItemControllerTests source from the upstream unit-test directory and references the real Management API project. It does not copy or reimplement the controller or helper. Run `dotnet test scripts/template-order-tests/TemplateOrder.Tests.csproj /p:UmbracoBuild=true` from the repository root. The project targets `net10.0`. The recorded run (2026-09-20, exit 0, about four minutes) also passed `/p:GitVersionBaseDirectory=<your path>/Umbraco-CMS` and `/p:GitRepoRoot=<your path>/Umbraco-CMS` so that Umbraco's version task could find a Git root. If your build stops in that task, add the same two properties with your own absolute path. Building referenced projects does not imply every upstream test or integration environment was exercised.

The four focused cases include the existing scrambled-order example and three new regressions for missing and extra hydration, duplicate identifiers with first-payload selection, and empty-page short-circuit behavior. Assertions preserve search Total independently of the number of returned entities and verify skipped service calls for an empty page.

Moq substitutes the search and template services. The mapper fixture projects key and alias. This establishes controller and shared-helper behavior under supplied results, not search-index quality, database consistency, or back-office browser authorization. No deployed CMS or user content store is modified by these unit checks.

The isolated staging copy omits Git history. The first two build attempts stopped in Umbraco's version task before tests ran. Subsequent commands supply GitVersionBaseDirectory and GitRepoRoot pointing to the original checkout for read-only version calculation; compilation still uses staged source. An intermediate run entered the back-office frontend install and was stopped. UmbracoBuild=true skipped those assets, but the full upstream test project still invoked a nested packaging-schema generator without the metadata arguments. The focused project avoids those unrelated packaging dependencies. Its Directory.Packages.props imports upstream test pins; an initial standalone restore exposed dependency-version conflicts, and an intermediate import location failed restore. The accepted run uses the corrected standard props file. The build also reports existing System.Security.Cryptography.Xml 10.0.9 advisories and upstream documentation warnings. This selection fix does not upgrade those dependencies, certify the full upstream test project, or claim a warning-free build.

The implementation was staged only after the immutable source baseline completed. Original snapshots and canonical hash checks accompany delivery. Earlier restore or build failures remain in the campaign record if any occur. The source-linked exercises explain the selection contract and its limits; reading them or passing the software tests does not establish human mastery. Broader upstream behavior or performance benchmarking would require additional evidence rather than being inferred from the focused suite.

## Source excerpt

From [Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs](../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs).

```cs
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.Template, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<TemplateItemResponseModel> { Total = searchResult.Total });
        }

        Guid[] keys = searchResult.Items.Select(x => x.Key).ToArray();
        IEnumerable<ITemplate> templates = await _templateService.GetAllAsync(keys);
        IEnumerable<ITemplate> orderedTemplates = OrderByRequestedIds(templates, keys);

        var result = new PagedModel<TemplateItemResponseModel>
        {
            Items = _mapper.MapEnumerable<ITemplate, TemplateItemResponseModel>(orderedTemplates),
            Total = searchResult.Total,
        };

        return Ok(result);
    }
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)

## Recorded command evidence

The accepted test evidence covers **4 distinct passing tests**. The commands below define the verified scope; repeated targeted runs do not increase the count.

| Check | Recorded command | Exit | Evidence |
|---|---|---:|---|
| `astra-netopen3-template-tests-r7` | `dotnet test scripts/template-order-tests/TemplateOrder.Tests.csproj --nologo --verbosity minimal /m:1 /p:UmbracoBuild=true /p:GitVersionBaseDirectory=<workstation path>/Umbraco-CMS /p:GitRepoRoot=<workstation path>/Umbraco-CMS --logger trx` | 0 | [record](evidence/astra-netopen3-template-tests-r7.json); the console log it names was not committed |

[Machine-readable results](evidence/regression.trx): `<Counters total="4" executed="4" passed="4" failed="0" …>`, with the four `Search_Template_Item_*` test names.
