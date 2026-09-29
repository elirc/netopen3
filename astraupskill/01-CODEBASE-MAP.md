# Connect search identifiers to hydrated responses

SearchTemplateItemController calls IEntitySearchService.Search with the template object type, query, skip, and take. If the returned page has no items, it returns an empty response carrying the search Total. The existing short circuit avoids hydration and mapping for that empty page.

For a nonempty page, the controller extracts the ordered keys and calls ITemplateService.GetAllAsync. That service returns full templates, whose order is not assumed. The shared ManagementApiControllerBase helper selects and orders them according to the requested keys before IUmbracoMapper creates response models.

The helper is shared with other management endpoints, so its behavior is expressed in terms of IEntity keys rather than template-specific aliases. Its dictionary retains the first hydrated entity for a key. Iterating and removing requested keys produces an ordered, duplicate-free subset without enumerating dictionary values as the output.

The controller test fixture substitutes search, hydration, and mapping. Its mapper projection retains key and alias, allowing tests to detect both ordering errors and selection of the wrong duplicate payload. When debugging, compare the requested key sequence, hydrated entity sequence, and mapped response separately. The total count belongs to the search result and can legitimately differ from the number of hydrated items on one page.

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
