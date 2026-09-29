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
