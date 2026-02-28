using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Tests;
using GlucoPilot.Nutrition.Data.Entities;
using GlucoPilot.Nutrition.Data.Repository;
using GlucoPilot.Nutrition.Endpoints.GetProduct;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework.Legacy;
using Endpoint = GlucoPilot.Nutrition.Endpoints.SearchProduct.Endpoint;

namespace GlucoPilot.Nutrition.Tests.Endpoints.SearchProduct;

[TestFixture]
public class SearchProductEndpointTests
{
    [Test]
    public async Task HandleAsync_Returns_Ok_With_Results()
    {
        var products = GenerateProducts().ToList();

        var expected = products.Take(50).Select(p => new ProductResponse
            { Id = p.Id, ProductName = p.ProductName, Code = p.Code, Nutriments = new NutrimentsResponse() });

    var repoMock = new Mock<IRepository<Product>>();
        repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(products));

        var result = await Endpoint.HandleAsync("search", null, repoMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;

        var actual = okResult!.Value!.ToList();
        Assert.That(actual, Has.Count.EqualTo(50));
        
        Assert.That(actual, Is.EqualTo(expected).AsCollection);
    }
    [Test]
    public async Task HandleAsync_Custom_Limit()
    {
        const int limit = 10;
        var products = GenerateProducts();
        var repoMock = new Mock<IRepository<Product>>();
        repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(products));

        var result = await Endpoint.HandleAsync("search", limit, repoMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;

        var actual = okResult!.Value!.ToList();
        Assert.That(actual, Has.Count.EqualTo(limit));
    }

    private static IEnumerable<Product> GenerateProducts(int count = 100)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Product { Id = $"{count}", ProductName = $"Product {count}", Code = $"{count}", Nutriments = new Nutriments() };
        }
    }
}