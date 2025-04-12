using FluentValidation;
using GlucoPilot.Api.Endpoints.Ingredients.GetIngredients;
using GlucoPilot.Api.Endpoints.Ingredients.List;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Ingredients.List
{
    [TestFixture]
    public class EndpointTests
    {
        private Mock<IValidator<ListIngredientsRequest>> _validatorMock;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Ingredient>> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<ListIngredientsRequest>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _repositoryMock = new Mock<IRepository<Ingredient>>();
        }

        [Test]
        public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new ListIngredientsRequest { Page = 0, PageSize = 10 };
            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                {
                    new FluentValidation.Results.ValidationFailure("Page", "Page must be greater than 0")
                }));

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        }

        [Test]
        public async Task HandleAsync_Returns_Ok_With_Ingredients_When_Request_Is_Valid()
        {
            var userId = Guid.NewGuid();
            var request = new ListIngredientsRequest { Page = 0, PageSize = 2 };
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Ingredient1", Created = DateTimeOffset.UtcNow, Uom = UnitOfMeasurement.Grams },
                new Ingredient { Id = Guid.NewGuid(), UserId = userId, Name = "Ingredient2", Created = DateTimeOffset.UtcNow, Uom = UnitOfMeasurement.Grams }
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _currentUserMock.Setup(c => c.GetUserId()).Returns(userId);
            _repositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(ingredients.AsQueryable());
            _repositoryMock
                .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredients.Count);

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<Ok<ListIngredientsResponse>>());
            var okResult = result.Result as Ok<ListIngredientsResponse>;
            Assert.That(okResult?.Value.Ingredients.Count, Is.EqualTo(2));
        }
    }
}