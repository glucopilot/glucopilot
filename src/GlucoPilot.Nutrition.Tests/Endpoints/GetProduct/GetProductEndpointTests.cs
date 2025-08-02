using GlucoPilot.Nutrition.Data.Entities;
using GlucoPilot.Nutrition.Data.Repository;
using GlucoPilot.Nutrition.Endpoints.GetProduct;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Nutrition.Tests.Endpoints.GetProduct;

[TestFixture]
public class GetProductEndpointTests
{
    [Test]
    public async Task HandleAsync_Returns_Ok_When_Product_Found()
    {
        var code = "123";
        var product = new Product
        {
            Id = "1",
            Code = code,
            ProductName = "Test Product",
            Nutriments = new Nutriments()
        };
        var repoMock = new Mock<IRepository<Product>>();
        repoMock.Setup(r => r.FindOneAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
            It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await Endpoint.HandleAsync(code, repoMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<ProductResponse>>());
        var okResult = result.Result as Ok<ProductResponse>;
        Assert.That(okResult!.Value.Id, Is.EqualTo(product.Id));
        Assert.That(okResult.Value.ProductName, Is.EqualTo(product.ProductName));
        Assert.That(okResult.Value.Code, Is.EqualTo(product.Code));
    }

    [Test]
    public async Task HandleAsync_Returns_NotFound_When_Product_Not_Found()
    {
        var code = "notfound";
        var repoMock = new Mock<IRepository<Product>>();
        repoMock.Setup(r => r.FindOneAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
            It.IsAny<FindOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await Endpoint.HandleAsync(code, repoMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<NotFound>());
    }
}
