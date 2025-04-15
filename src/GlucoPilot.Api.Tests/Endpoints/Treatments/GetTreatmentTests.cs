using NUnit.Framework;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.Api.Endpoints.Treatments.GetTreatment;

namespace GlucoPilot.Api.Endpoints.Treatments
{
    [TestFixture]
    public class GetTreatmentTests
    {
        private Mock<ICurrentUser> _mockCurrentUser;
        private Mock<IRepository<Treatment>> _mockTreatmentRepository;

        [SetUp]
        public void SetUp()
        {
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockTreatmentRepository = new Mock<IRepository<Treatment>>();
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Treatment_Exists_Injection_Null()
        {
            var treatmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mealId = Guid.NewGuid();
            var ingredientId = Guid.NewGuid();
            var treatment = new Treatment
            {
                Id = treatmentId,
                UserId = userId,
                Created = DateTimeOffset.UtcNow,
                MealId = mealId,
                Meal = new Meal
                {
                    Id = mealId,
                    Name = "Test Meal",
                    Created = DateTimeOffset.UtcNow,
                    MealIngredients = new[]
                    {
                        new MealIngredient
                        {
                            Ingredient = new Ingredient { Id = ingredientId, Created = DateTimeOffset.UtcNow, Name = "Sugar", Uom = UnitOfMeasurement.Unit, Calories = 100, Carbs = 20, Protein = 10, Fat = 5 },
                            Quantity = 2,
                            Id = Guid.NewGuid(),
                            MealId = mealId,
                            IngredientId = ingredientId
                        }
                    }
                }
            };

            _mockCurrentUser.Setup(x => x.GetUserId()).Returns(userId);
            _mockTreatmentRepository
                .Setup(x => x.GetAll(It.IsAny<FindOptions>()))
                .Returns(new[] { treatment }.AsQueryable());

            var result = await Endpoint.HandleAsync(
                treatmentId,
                _mockCurrentUser.Object,
                _mockTreatmentRepository.Object,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                var okResult = result.Result as Ok<GetTreatmentResponse>;
                Assert.That(okResult, Is.InstanceOf<Ok<GetTreatmentResponse>>());
                Assert.That(okResult?.Value.Id, Is.EqualTo(treatmentId));
                Assert.That(okResult.Value.Meal.Id, Is.EqualTo(mealId));
            });            
        }

        [Test]
        public async Task HandleAsync_Should_Return_Ok_When_Treatment_Exists_Meal_Null()
        {
            var treatmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var injectionId = Guid.NewGuid();
            var insulinId = Guid.NewGuid();
            var treatment = new Treatment
            {
                Id = treatmentId,
                UserId = userId,
                Created = DateTimeOffset.UtcNow,
                MealId = null,
                Meal = null,
                InjectionId = injectionId,
                Injection = new Injection
                {
                    Id = injectionId,
                    Created = DateTimeOffset.UtcNow,
                    InsulinId = Guid.NewGuid(),
                    Units = 10,
                    Insulin = new Insulin
                    {
                        Id = insulinId,
                        Created = DateTimeOffset.UtcNow,
                        Name = "Test Insulin",
                        Type = InsulinType.Basal
                    }
                }
            };

            _mockCurrentUser.Setup(x => x.GetUserId()).Returns(userId);
            _mockTreatmentRepository
                .Setup(x => x.GetAll(It.IsAny<FindOptions>()))
                .Returns(new[] { treatment }.AsQueryable());

            var result = await Endpoint.HandleAsync(
                treatmentId,
                _mockCurrentUser.Object,
                _mockTreatmentRepository.Object,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                var okResult = result.Result as Ok<GetTreatmentResponse>;
                Assert.That(okResult, Is.InstanceOf<Ok<GetTreatmentResponse>>());
                Assert.That(okResult?.Value.Id, Is.EqualTo(treatmentId));
                Assert.That(okResult.Value.Injection.Id, Is.EqualTo(injectionId));
            });            
        }

        [Test]
        public void HandleAsync_Should_Throw_NotFoundException_When_Treatment_Does_Not_Exist()
        {
            var treatmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.GetUserId()).Returns(userId);
            _mockTreatmentRepository.Setup(x => x.GetAll(It.IsAny<FindOptions>()))
                .Returns(Enumerable.Empty<Treatment>().AsQueryable());

            Assert.ThrowsAsync<NotFoundException>(async () =>
                await Endpoint.HandleAsync(
                    treatmentId,
                    _mockCurrentUser.Object,
                    _mockTreatmentRepository.Object,
                    CancellationToken.None));
        }

        [Test]
        public async Task HandleAsync_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
        {
            var treatmentId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated()).Returns(false);

            Assert.That(async () => await Endpoint.HandleAsync(
                treatmentId,
                _mockCurrentUser.Object,
                _mockTreatmentRepository.Object,
                CancellationToken.None), Throws.InstanceOf<NotFoundException>().With.Message.EqualTo("TREATMENT_NOT_FOUND"));
        }
    }
}