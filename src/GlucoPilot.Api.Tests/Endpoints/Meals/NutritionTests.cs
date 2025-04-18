using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Meals.Nutrition;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using Endpoint = GlucoPilot.Api.Endpoints.Meals.Nutrition.Endpoint;

namespace GlucoPilot.Api.Tests.Endpoints.Meals
{
    [TestFixture]
    public class NutritionTests
    {
        private Mock<IValidator<NutritionRequestModel>> _validatorMock;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Treatment>> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<NutritionRequestModel>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _repositoryMock = new Mock<IRepository<Treatment>>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new NutritionRequestModel();
            var validationResult = new ValidationResult([
                new ValidationFailure("Property", "Error message")
            ]);

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        }

        [Test]
        public void HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
        {
            var mockValidator = new Mock<IValidator<NutritionRequestModel>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<NutritionRequestModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _currentUserMock.Setup(x => x.GetUserId()).Throws(new UnauthorizedException("USER_NOT_LOGGED_IN"));

            var mockRepository = new Mock<IRepository<Meal>>();

            var request = new NutritionRequestModel { From = DateTimeOffset.UtcNow.AddDays(-1), To = DateTimeOffset.UtcNow };

            Assert.That(() => Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None),
                Throws.InstanceOf<UnauthorizedException>().With.Message.EqualTo("USER_NOT_LOGGED_IN"));
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_With_Correct_NutritionResponseModel()
        {
            var userId = Guid.NewGuid();
            var request = new NutritionRequestModel { From = DateTimeOffset.UtcNow.AddDays(-1), To = DateTimeOffset.UtcNow };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            var treatments = new List<Treatment>
            {
                new Treatment
                {
                    UserId = userId,
                    Created = DateTimeOffset.UtcNow.AddMinutes(-1),
                    Meal = new Meal
                    {
                        Created = DateTimeOffset.UtcNow.AddMinutes(-1),
                        Name = "Breakfast",
                        MealIngredients = new List<MealIngredient>
                        {
                            new MealIngredient
                            {
                                Ingredient = new Ingredient
                                {
                                    Created = DateTimeOffset.UtcNow.AddMinutes(-1),
                                    Name = "Egg",
                                    Uom = UnitOfMeasurement.Grams,
                                    Calories = 100,
                                    Carbs = 20,
                                    Protein = 10,
                                    Fat = 5
                                },
                                Quantity = 2,
                                Id = default,
                                MealId = default,
                                IngredientId = default
                            }
                        }
                    }
                }
            };

            _repositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(treatments.AsQueryable());

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<Ok<NutritionResponseModel>>());
            var response = (result.Result as Ok<NutritionResponseModel>)?.Value;
            Assert.That(response, Is.Not.Null);
            Assert.That(response.TotalCalories, Is.EqualTo(200));
            Assert.That(response.TotalCarbs, Is.EqualTo(40));
            Assert.That(response.TotalProtein, Is.EqualTo(20));
            Assert.That(response.TotalFat, Is.EqualTo(10));
        }

        [Test]
        public async Task HandleAsync_Should_Ignore_Treatments_With_No_Meals()
        {
            var userId = Guid.NewGuid();
            var request = new NutritionRequestModel { From = DateTimeOffset.UtcNow.AddDays(-1), To = DateTimeOffset.UtcNow };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _currentUserMock
                .Setup(c => c.GetUserId())
                .Returns(userId);

            var treatments = new List<Treatment>
            {
                new Treatment
                {
                    UserId = userId,
                    Created = DateTimeOffset.UtcNow.AddMinutes(-1),
                    Injection = new Injection()
                    {
                        Created = DateTimeOffset.UtcNow.AddMinutes(-1),
                        InsulinId = Guid.NewGuid(),
                        Units = 1,
                    }
                }
            };

            _repositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Treatment, bool>>>(), It.IsAny<FindOptions>()))
                .Returns(treatments.AsQueryable());

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<Ok<NutritionResponseModel>>());
            var response = (result.Result as Ok<NutritionResponseModel>)?.Value;
            Assert.That(response, Is.Not.Null);
            Assert.That(response.TotalCalories, Is.EqualTo(0));
            Assert.That(response.TotalCarbs, Is.EqualTo(0));
            Assert.That(response.TotalProtein, Is.EqualTo(0));
            Assert.That(response.TotalFat, Is.EqualTo(0));
        }
    }
}