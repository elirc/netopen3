# Preserve requested identity and search ordering

Umbraco template search first returns ordered entity identifiers, then hydrates full templates through another service. Hydration order need not match search order, and the two result sets can differ. The response should retain requested order, omit missing entities, and exclude entities that were not requested.

The shared OrderByRequestedIds helper previously sorted hydrated entities by Array.IndexOf. An unrequested identifier received index minus one and moved ahead of legitimate results. Duplicate hydrated rows could also survive, while repeated index searches added unnecessary work. The repair builds a first-entity lookup and iterates requested identifiers, removing each match after use.

The new contract returns each available requested identifier once, in its first requested position, using the first hydrated payload for duplicate entities. Template-controller regressions cover missing and extra entities, duplicate identifiers and payloads, and the existing empty-page short circuit. Search Total remains the search service's metadata rather than being replaced by hydrated item count.

Read the map and identity concepts, then follow the worked helper. Practice and its separate answers explore mismatched result sets. Verification records the focused unit test command and its limits. These tests use service and mapper substitutes; they do not establish search-index freshness, database integration, or back-office browser authorization.

## Source excerpt

From [Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs](../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs).

```cs
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Template.Item;

[TestFixture]
public class SearchTemplateItemControllerTests
{
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<ITemplateService> _templateService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private SearchTemplateItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _templateService = new Mock<ITemplateService>();
        _mapper = new Mock<IUmbracoMapper>();
        _controller = new SearchTemplateItemController(
            _entitySearchService.Object,
            _templateService.Object,
            _mapper.Object);
    }

    [Test]
    public async Task Search_Template_Item_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.Template, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Service returns templates in scrambled order [C, A, B]
        ITemplate templateC = Mock.Of<ITemplate>(x => x.Key == keyC);
        ITemplate templateA = Mock.Of<ITemplate>(x => x.Key == keyA);
        ITemplate templateB = Mock.Of<ITemplate>(x => x.Key == keyB);
        _templateService
            .Setup(x => x.GetAllAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new[] { templateC, templateA, templateB });

        _mapper
            .Setup(x => x.MapEnumerable<ITemplate, TemplateItemResponseModel>(It.IsAny<IEnumerable<ITemplate>>()))
            .Returns<IEnumerable<ITemplate>>(entities =>
                entities.Select(e => new TemplateItemResponseModel { Alias = e.Alias, Id = e.Key }).ToList());

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<TemplateItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<TemplateItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }

    [Test]
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

```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
