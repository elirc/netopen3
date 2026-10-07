# Consume the lookup one requested key at a time

Draw a hydration column and an output column. Insert A-first into the lookup, then B-first. Attempt to insert B-later and A-later and record that both attempts leave the first values unchanged. Add an extra entity whose key never appears in the requested sequence.

Now iterate requested keys B, missing, A, B. Remove B and append B-first. Missing is absent, so append nothing. Remove A and append A-first. The repeated B is now absent because its entry was consumed earlier. The extra lookup entry remains unused. The final output is exactly B-first, A-first.

Pass those entities through the fixture mapper and inspect identifiers and aliases in the response. Copy Total from the search page without substituting the output length. This separates entity selection from pagination metadata and makes every operation in the algorithm observable in the controller regression.

Repeat the trace with an empty requested page. The controller returns before building keys or hydrating templates, so the helper is not invoked. Verify the mapper spy remains unused as well. Finally, replace the helper with the original sort (the one-liner in [03](03-WORKED-CHANGE.md#before-and-after)) in a disposable copy. Trace this lab's own inputs through it first. Hydrated A-first, B-first, B-later, A-later, extra against requested B, missing, A, B gives indices 2, 0, 0, 2, -1, so the stable sort emits extra, B-first, B-later, A-first, A-later. Then run the focused project and confirm that exactly the extra/missing and duplicate tests fail ([06](06-SOLUTIONS-AND-REVIEW.md#break-it-answers-derived-by-reading-the-tests-2026-10-06) lists the assertions). Restore the reviewed implementation with `git checkout -- Umbraco-CMS` before running acceptance.

## Source excerpt

From [Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs](../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs).

```cs
    public async Task Search_Template_Item_Empty_Page_Preserves_Total_Without_Hydration_Or_Mapping()
    {
        SetSearchResult([], 17);

        PagedModel<TemplateItemResponseModel> result = await SearchResult();

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.Total, Is.EqualTo(17));
        });
        _templateService.Verify(x => x.GetAllAsync(It.IsAny<Guid[]>()), Times.Never);
        _mapper.Verify(x => x.MapEnumerable<ITemplate, TemplateItemResponseModel>(It.IsAny<IEnumerable<ITemplate>>()), Times.Never);
    }

    private void SetSearchResult(Guid[] keys, long total)
    {
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.Template, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = keys.Select(key => (IEntitySlim)new EntitySlim { Key = key }).ToArray(),
                Total = total,
            });
        _mapper
            .Setup(x => x.MapEnumerable<ITemplate, TemplateItemResponseModel>(It.IsAny<IEnumerable<ITemplate>>()))
            .Returns<IEnumerable<ITemplate>>(entities =>
                entities.Select(entity => new TemplateItemResponseModel { Alias = entity.Alias, Id = entity.Key }).ToList());
    }

    private static ITemplate Template(Guid key, string alias) =>
        Mock.Of<ITemplate>(template => template.Key == key && template.Alias == alias);

    private async Task<PagedModel<TemplateItemResponseModel>> SearchResult()
    {
        IActionResult result = await _controller.Search(CancellationToken.None, "test");
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var value = ((OkObjectResult)result).Value;
        Assert.That(value, Is.TypeOf<PagedModel<TemplateItemResponseModel>>());
        return (PagedModel<TemplateItemResponseModel>)value!;
    }
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
