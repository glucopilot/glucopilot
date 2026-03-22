using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Tests;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Nutrition.Data.Entities;
using GlucoPilot.Nutrition.Data.Repository;
using GlucoPilot.Nutrition.Endpoints.GetProduct;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework.Legacy;
using Endpoint = GlucoPilot.Nutrition.Endpoints.SearchProduct.Endpoint;
using GPRepository = GlucoPilot.Data.Repository;

namespace GlucoPilot.Nutrition.Tests.Endpoints.SearchProduct;

[TestFixture]
public class SearchProductEndpointTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private Mock<IRepository<Product>> _repoMock;
    private Mock<GPRepository.IRepository<Ingredient>> _gpRepoMock;
    private Mock<ICurrentUser> _currentUserMock;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IRepository<Product>>();
        _gpRepoMock = new Mock<GPRepository.IRepository<Ingredient>>();
        _currentUserMock = new Mock<ICurrentUser>();

        _currentUserMock.Setup(c => c.GetUserId()).Returns(_userId);
    }

    [Test]
    public async Task HandleAsync_Returns_Ok_With_Results()
    {
        var products = GenerateProducts().ToList();

        var expected = products.Take(50).Select(p => new ProductResponse
        { Id = p.Id, ProductName = p.ProductName, Code = p.Code, Nutriments = new NutrimentsResponse() });

        _repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(products));

        var ingredients = GenerateIngredients(0);
        _gpRepoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Ingredient, bool>>>(),
                It.IsAny<GPRepository.FindOptions>()))
            .Returns(new TestAsyncEnumerable<Ingredient>(ingredients));

        var result = await Endpoint.HandleAsync("search", null, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;

        var actual = okResult!.Value!.ToList();
        Assert.That(actual, Has.Count.EqualTo(50));

        Assert.That(actual, Is.EqualTo(expected).AsCollection);
    }

    [TestCase(10, ExpectedResult = 10)]
    [TestCase(0, ExpectedResult = 50)]
    [TestCase(-10, ExpectedResult = 50)]
    [TestCase(200, ExpectedResult = 100)]
    public async Task<int> HandleAsync_Custom_Limit(int limit)
    {
        var products = GenerateProducts();
        _repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(products));

        var ingredients = GenerateIngredients(0);
        _gpRepoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Ingredient, bool>>>(),
                It.IsAny<GPRepository.FindOptions>()))
            .Returns(new TestAsyncEnumerable<Ingredient>(ingredients));

        var result = await Endpoint.HandleAsync("search", limit, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;

        var actual = okResult!.Value!.ToList();
        return actual.Count;
    }

    [Test]
    public async Task HandleAsync_Filters_Products_Where_Ingredient_Has_The_Same_Barcode()
    {
        var products = GenerateProducts(20);
        _repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(products));

        var ingredients = GenerateIngredients(10);
        _gpRepoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Ingredient, bool>>>(),
                It.IsAny<GPRepository.FindOptions>()))
            .Returns(new TestAsyncEnumerable<Ingredient>(ingredients));

        var result = await Endpoint.HandleAsync("search", null, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;
        var actual = okResult!.Value!.ToList();
        Assert.That(actual, Has.Count.EqualTo(10));
    }

    private static IEnumerable<Product> GenerateProducts(int count = 100)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Product { Id = $"{i}", ProductName = $"Product {i}", Code = $"{i}", Nutriments = new Nutriments() };
        }
    }

    private static IEnumerable<Ingredient> GenerateIngredients(int count = 10)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Ingredient { Id = Guid.NewGuid(), Barcode = $"{i}", Created = DateTime.Now, Name = $"Ingredient {i}", Uom = GlucoPilot.Data.Enums.UnitOfMeasurement.Grams };
        }
    }
}