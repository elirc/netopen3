# Assert identity, payload, metadata, and skipped calls

The existing normal-order test returns search keys A, B, C and hydrates C, A, B. It verifies the basic ordering contract. The new missing-and-extra case requests A, a missing key, and B, then hydrates B, an extra key, and A. It requires exactly A and B and preserves the supplied total of nineteen.

The duplicate case repeats both requested identifiers and hydrated entities. Distinct aliases on the first and later payloads make the winner observable. Assertions inspect the resulting identifiers and aliases, so an implementation that deduplicates by keeping the last payload cannot pass accidentally.

The empty-page case supplies no search items with a nonzero total. It requires an empty response with that total and verifies hydration and mapping were never called. This protects the controller's existing short circuit rather than claiming a newly introduced optimization.

These are focused controller unit tests. Search and template services are Moq substitutes, and mapping is a small projection. They cannot prove production index ordering, SQL behavior, or browser permissions. If a test fails, identify which sequence or boundary differs before broadening the fix. An item-count-only assertion would miss order and duplicate-payload errors, while an order-only assertion could miss an extra entity at the front.

## Source excerpt

From [Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs](../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs).

```cs
    public async Task Search_Template_Item_Excludes_Unrequested_And_Missing_Entities_Without_Changing_Total()
    {
        var keyA = Guid.NewGuid();
        var missing = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var extra = Guid.NewGuid();
        SetSearchResult([keyA, missing, keyB], 19);
        _templateService.Setup(x => x.GetAllAsync(It.IsAny<Guid[]>())).ReturnsAsync(
            new[] { Template(keyB, "b"), Template(extra, "extra"), Template(keyA, "a") });

        PagedModel<TemplateItemResponseModel> result = await SearchResult();

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(x => x.Id), Is.EqualTo(new[] { keyA, keyB }));
            Assert.That(result.Total, Is.EqualTo(19));
        });
    }

    [Test]
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
