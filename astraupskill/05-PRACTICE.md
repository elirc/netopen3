# Work through mismatched result sets

Exercise one: requested identifiers are A, missing, B, while hydrated entities arrive as B, extra, A. Predict the old Array.IndexOf sort order and the corrected result. Explain why assigning a high sort index to extras would still fail the requirement to exclude them.

Exercise two: request B, A, B, A. Hydration returns A-first, B-first, B-later, A-later. Predict both identifiers and aliases in the response. Identify which dictionary operation chooses the first payload and which operation prevents repeated output.

Exercise three: return an empty search page with Total seventeen. Predict the response Total and the number of hydration and mapper calls. Explain why replacing Total with Items.Count would change pagination meaning.

Exercise four: keep the requested sequence fixed but reverse hydration order for distinct identifiers. Predict whether the response order changes. Then repeat with duplicate payloads and explain why reversing hydration can change the selected alias under the explicit first-payload policy.

Exercise five: consider a large batch. Compare repeated Array.IndexOf lookups with one dictionary construction and one requested-ID pass. State the average complexity assumption and the memory tradeoff. Keep performance reasoning separate from claims about authorization or search-index consistency, which this helper does not establish.

## Source excerpt

From [Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs](../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs).

```cs
    public async Task Search_Template_Item_Collapses_Duplicate_Keys_And_Keeps_First_Hydrated_Entity()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        SetSearchResult([keyB, keyA, keyB, keyA], 4);
        _templateService.Setup(x => x.GetAllAsync(It.IsAny<Guid[]>())).ReturnsAsync(
            new[] { Template(keyA, "a-first"), Template(keyB, "b-first"), Template(keyB, "b-later"), Template(keyA, "a-later") });

        PagedModel<TemplateItemResponseModel> result = await SearchResult();

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(x => x.Id), Is.EqualTo(new[] { keyB, keyA }));
            Assert.That(result.Items.Select(x => x.Alias), Is.EqualTo(new[] { "b-first", "a-first" }));
            Assert.That(result.Total, Is.EqualTo(4));
        });
    }

    [Test]
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
