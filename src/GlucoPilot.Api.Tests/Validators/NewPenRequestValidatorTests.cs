using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Pens.NewPen;
using GlucoPilot.Api.Models;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Validators
{
    [TestFixture]
    public class NewPenRequestValidatorTests
    {
        private NewPenRequest.NewPenRequestValidator _validator;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IRepository<Insulin>> _insulinRepositoryMock;
        private Guid _userId;

        [SetUp]
        public void SetUp()
        {
            _userId = Guid.NewGuid();
            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.GetUserId()).Returns(_userId);
            _insulinRepositoryMock = new Mock<IRepository<Insulin>>();

            _validator = new NewPenRequest.NewPenRequestValidator(_currentUserMock.Object, _insulinRepositoryMock.Object);
        }

        [Test]
        public async Task Validator_Passes_With_Valid_Values()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = PenColour.Blue,
                Serial = "SN123456"
            };

            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Insulin, bool>>>(),
                    It.IsAny<FindOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Insulin { Id = request.InsulinId, UserId = _userId, Name = "Test", Type = (Data.Enums.InsulinType)InsulinType.Bolus });

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public async Task Validator_Fails_When_Model_Is_Invalid()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = (PenModel)99,
                Colour = PenColour.Blue,
                Serial = "SN123456"
            };

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Model), Is.Not.Null);
        }

        [Test]
        public async Task Validator_Fails_When_Colour_Is_Invalid()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = (PenColour)99,
                Serial = "SN123456"
            };

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Colour), Is.Not.Null);
        }

        [Test]
        public async Task Validator_Fails_When_Serial_Is_Empty()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = PenColour.Blue,
                Serial = ""
            };

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }

        [Test]
        public async Task Validator_Fails_When_Serial_Is_Whitespace()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = PenColour.Blue,
                Serial = "   "
            };

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }

        [Test]
        public async Task Validator_Fails_When_Serial_Too_Long()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = PenColour.Blue,
                Serial = new string('A', 101)
            };

            var result = await _validator.TestValidateAsync(request).ConfigureAwait(false);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }

        [Test]
        public async Task Validator_Fails_When_InsulinId_Not_Found()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovoPen6,
                Colour = PenColour.Blue,
                Serial = "SN123456"
            };

            _insulinRepositoryMock
                .Setup(r => r.FindOneAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Insulin, bool>>>(),
                    It.IsAny<FindOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Insulin)null);

            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.InsulinId);
        }
    }
}
