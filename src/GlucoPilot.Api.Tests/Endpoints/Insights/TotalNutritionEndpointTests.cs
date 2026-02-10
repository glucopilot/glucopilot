using FluentValidation;
using FluentValidation.Results;
using GlucoPilot.Api.Endpoints.Insights.TotalNutrition;
using GlucoPilot.AspNetCore.Exceptions;
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
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints.Insights
{
    [TestFixture]
    internal class TotalNutritionEndpointTests
    {
        private Mock<IValidator<TotalNutritionRequest>> _validatorMock = null!;
        private Mock<ICurrentUser> _currentUserMock = null!;
        private Mock<IRepository<Treatment>> _repositoryMock = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<TotalNutritionRequest>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _repositoryMock = new Mock<IRepository<Treatment>>();
        }

        [Test]
        public async Task HandleAsync_Returns_ValidationProblem_When_Request_Is_Invalid()
        {
            var request = new TotalNutritionRequest();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("From", "Invalid") }));

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

            Assert.That(result.Result, Is.InstanceOf<ValidationProblem>());
        }

        [Test]
        public async Task HandleAsync_Returns_Ok_With_Correct_Nutrition_Totals()
        {
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(cu => cu.GetUserId()).Returns(userId);
            var mealId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();
            var treatments = new List<Treatment>
            {
                new Treatment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Created = DateTimeOffset.UtcNow,
                    InjectionId = null,
                    Meals = [new TreatmentMeal
                    {
                        TreatmentId = Guid.NewGuid(),
                        MealId = Guid.NewGuid(),
                        Meal = new Meal
                        {
                            Id = mealId,
                            Name = "Lunch",
                            Created = DateTimeOffset.UtcNow,
                            MealIngredients = new List<MealIngredient>
                            {
                                new MealIngredient
                                {
                                    Ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "Sugar", Uom = UnitOfMeasurement.Grams, Calories = 100, Carbs = 20, Protein = 10, Fat = 5 },
                                    Quantity = 2,
                                    Id = Guid.NewGuid(),
                                    IngredientId = ingredientId,
                                    MealId = mealId
                                }
                            }
                        },
                        Quantity = 2
                    }],
                    Ingredients = [new TreatmentIngredient
                    {
                        Id = Guid.NewGuid(),
                        IngredientId = ingredientId,
                        TreatmentId = Guid.NewGuid(),
                        Ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "Sugar", Uom = UnitOfMeasurement.Grams, Calories = 100, Carbs = 20, Protein = 10, Fat = 5 },
                        Quantity = 2
                    }],
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TotalNutritionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock
                .Setup(r => r.GetAll(It.IsAny<FindOptions>()))
                .Returns(treatments.AsQueryable());

            var request = new TotalNutritionRequest { From = DateTimeOffset.UtcNow.AddDays(-1), To = DateTimeOffset.UtcNow };

            var result = await Endpoint.HandleAsync(request, _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object);

            Assert.That(result.Result, Is.InstanceOf<Ok<TotalNutritionResponse>>());
            var response = ((Ok<TotalNutritionResponse>)result.Result).Value;
            Assert.That(response.TotalCalories, Is.EqualTo(600));
            Assert.That(response.TotalCarbs, Is.EqualTo(120));
            Assert.That(response.TotalProtein, Is.EqualTo(60));
            Assert.That(response.TotalFat, Is.EqualTo(30));
        }

        [Test]
        public async Task HandleAsync_Returns_Unauthorized_When_User_Is_Not_Authenticated()
        {
            var exception = new UnauthorizedException("Test");
            _currentUserMock.Setup(cu => cu.GetUserId()).Throws(exception);

            Assert.That(() => Endpoint.HandleAsync(new TotalNutritionRequest(), _validatorMock.Object, _currentUserMock.Object, _repositoryMock.Object, CancellationToken.None), Throws.Exception.SameAs(exception));
        }
    }
}
