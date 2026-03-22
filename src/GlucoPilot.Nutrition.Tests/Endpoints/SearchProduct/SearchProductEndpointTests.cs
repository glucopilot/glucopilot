using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
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
    public async Task HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN")); ;

        Assert.That(() => Endpoint.HandleAsync("search", null, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None),
            Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
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

        var ingredients = GenerateIngredients(_userId, 0);
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

        var ingredients = GenerateIngredients(_userId, 0);
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

        var ingredients = GenerateIngredients(_userId, 10);
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

    [Test]
    public async Task HandleAsync_Returns_Correct_ProductResponse_For_Product()
    {
        var product = new Product
        {
            Id = "p1",
            ProductType = "Food",
            Quantity = "1",
            ProductQuantityUnit = "g",
            ProductName = "Test Product",
            ProductQuantity = 123.45,
            NutritionDataPer = "100g",
            NutritionDataPreparedPer = "prepared",
            Code = "ABC123",
            ServingQuantity = 50.5,
            Nutriments = new Nutriments
            {
                EnergyUnit = "kJ",
                FatUnit = "g",
                CarbohydratesUnit = "g",
                EnergyKcalUnit = "kcal",
                EnergyKcalValue = 200.1,
                EnergyValue = 800.2,
                CarbohydratesValue = 30.3,
                Proteins = 10.4f,
                EnergyKcalValueComputed = 201.5,
                ProteinsValue = 11.6f,
                EnergyKcal = 202.7,
                ProteinsUnit = "g",
                Carbohydrates = 31.8,
                Energy = 900.9f,
                Fat = 5.5f,
                FatValue = 6.6f
            }
        };

        _repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(new[] { product }));
        _gpRepoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Ingredient, bool>>>(),
                It.IsAny<GPRepository.FindOptions>()))
            .Returns(new TestAsyncEnumerable<Ingredient>(GenerateIngredients(_userId, 0)));

        var result = await Endpoint.HandleAsync("Test Product", 1, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;
        var actual = okResult!.Value!.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.Id, Is.EqualTo(product.Id));
            Assert.That(actual.ProductType, Is.EqualTo(product.ProductType));
            Assert.That(actual.Quantity, Is.EqualTo(product.Quantity));
            Assert.That(actual.ProductQuantityUnit, Is.EqualTo(product.ProductQuantityUnit));
            Assert.That(actual.ProductName, Is.EqualTo(product.ProductName));
            Assert.That(actual.ProductQuantity, Is.EqualTo(product.ProductQuantity));
            Assert.That(actual.NutritionDataPer, Is.EqualTo(product.NutritionDataPer));
            Assert.That(actual.NutritionDataPreparedPer, Is.EqualTo(product.NutritionDataPreparedPer));
            Assert.That(actual.Code, Is.EqualTo(product.Code));
            Assert.That(actual.ServingQuantity, Is.EqualTo(product.ServingQuantity));
            var n = actual.Nutriments;
            Assert.That(n, Is.Not.Null);
            Assert.That(n!.EnergyUnit, Is.EqualTo(product.Nutriments.EnergyUnit));
            Assert.That(n.FatUnit, Is.EqualTo(product.Nutriments.FatUnit));
            Assert.That(n.CarbohydratesUnit, Is.EqualTo(product.Nutriments.CarbohydratesUnit));
            Assert.That(n.EnergyKcalUnit, Is.EqualTo(product.Nutriments.EnergyKcalUnit));
            Assert.That(n.EnergyKcalValue, Is.EqualTo(product.Nutriments.EnergyKcalValue));
            Assert.That(n.EnergyValue, Is.EqualTo(product.Nutriments.EnergyValue));
            Assert.That(n.CarbohydratesValue, Is.EqualTo(product.Nutriments.CarbohydratesValue));
            Assert.That(n.Proteins, Is.EqualTo(product.Nutriments.Proteins));
            Assert.That(n.EnergyKcalValueComputed, Is.EqualTo(product.Nutriments.EnergyKcalValueComputed));
            Assert.That(n.ProteinsValue, Is.EqualTo(product.Nutriments.ProteinsValue));
            Assert.That(n.EnergyKcal, Is.EqualTo(product.Nutriments.EnergyKcal));
            Assert.That(n.ProteinsUnit, Is.EqualTo(product.Nutriments.ProteinsUnit));
            Assert.That(n.Carbohydrates, Is.EqualTo(product.Nutriments.Carbohydrates));
            Assert.That(n.Energy, Is.EqualTo(product.Nutriments.Energy));
            Assert.That(n.Fat, Is.EqualTo(product.Nutriments.Fat));
            Assert.That(n.FatValue, Is.EqualTo(product.Nutriments.FatValue));
        }
        ;
    }

    [Test]
    public async Task HandleAsync_Maps_Null_Nutriments_To_Null_Fields()
    {
        var product = new Product
        {
            Id = "p2",
            ProductType = null,
            Quantity = null,
            ProductQuantityUnit = null,
            ProductName = "No Nutriments",
            ProductQuantity = null,
            NutritionDataPer = null,
            NutritionDataPreparedPer = null,
            Code = "CODE2",
            ServingQuantity = null,
            Nutriments = null
        };
        _repoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(new TestAsyncEnumerable<Product>(new[] { product }));
        _gpRepoMock.Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Ingredient, bool>>>(),
                It.IsAny<GPRepository.FindOptions>()))
            .Returns(new TestAsyncEnumerable<Ingredient>(GenerateIngredients(_userId, 0)));

        var result = await Endpoint.HandleAsync("No Nutriments", 1, _repoMock.Object, _gpRepoMock.Object, _currentUserMock.Object, CancellationToken.None);
        Assert.That(result.Result, Is.TypeOf<Ok<IEnumerable<ProductResponse>>>());
        var okResult = result.Result as Ok<IEnumerable<ProductResponse>>;
        var actual = okResult!.Value!.Single();
        Assert.That(actual.Nutriments, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.Nutriments!.EnergyUnit, Is.Null);
            Assert.That(actual.Nutriments.FatUnit, Is.Null);
            Assert.That(actual.Nutriments.CarbohydratesUnit, Is.Null);
            Assert.That(actual.Nutriments.EnergyKcalUnit, Is.Null);
            Assert.That(actual.Nutriments.EnergyKcalValue, Is.Null);
            Assert.That(actual.Nutriments.EnergyValue, Is.Null);
            Assert.That(actual.Nutriments.CarbohydratesValue, Is.Null);
            Assert.That(actual.Nutriments.Proteins, Is.Null);
            Assert.That(actual.Nutriments.EnergyKcalValueComputed, Is.Null);
            Assert.That(actual.Nutriments.ProteinsValue, Is.Null);
            Assert.That(actual.Nutriments.EnergyKcal, Is.Null);
            Assert.That(actual.Nutriments.ProteinsUnit, Is.Null);
            Assert.That(actual.Nutriments.Carbohydrates, Is.Null);
            Assert.That(actual.Nutriments.Energy, Is.Null);
            Assert.That(actual.Nutriments.Fat, Is.Null);
            Assert.That(actual.Nutriments.FatValue, Is.Null);
        }
        ;
    }

    private static IEnumerable<Product> GenerateProducts(int count = 100)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Product { Id = $"{i}", ProductName = $"Product {i}", Code = $"{i}", Nutriments = new Nutriments() };
        }
    }

    private static IEnumerable<Ingredient> GenerateIngredients(Guid userId, int count = 10)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Ingredient { Id = Guid.NewGuid(), UserId = userId, Barcode = $"{i}", Created = DateTimeOffset.Now, Name = $"Ingredient {i}", Uom = GlucoPilot.Data.Enums.UnitOfMeasurement.Grams };
        }
    }
}