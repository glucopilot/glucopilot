using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Pens.NewPen;
using GlucoPilot.Api.Models;
using NUnit.Framework;
using System;

namespace GlucoPilot.Api.Tests.Validators
{
    [TestFixture]
    public class NewPenRequestValidatorTests
    {
        private NewPenRequest.NewPenRequestValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new NewPenRequest.NewPenRequestValidator();
        }

        [Test]
        public void Validator_Passes_With_Valid_Values()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovePen6,
                Colour = PenColour.Blue,
                Serial = "SN123456"
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Validator_Fails_When_Model_Is_Invalid()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = (PenModel)99,
                Colour = PenColour.Blue,
                Serial = "SN123456"
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Model), Is.Not.Null);
        }

        [Test]
        public void Validator_Fails_When_Colour_Is_Invalid()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovePen6,
                Colour = (PenColour)99,
                Serial = "SN123456"
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Colour), Is.Not.Null);
        }

        [Test]
        public void Validator_Fails_When_Serial_Is_Empty()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovePen6,
                Colour = PenColour.Blue,
                Serial = ""
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }

        [Test]
        public void Validator_Fails_When_Serial_Is_Whitespace()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovePen6,
                Colour = PenColour.Blue,
                Serial = "   "
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }

        [Test]
        public void Validator_Fails_When_Serial_Too_Long()
        {
            var request = new NewPenRequest
            {
                InsulinId = Guid.NewGuid(),
                Model = PenModel.NovePen6,
                Colour = PenColour.Blue,
                Serial = new string('A', 101)
            };

            var result = _validator.TestValidate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ShouldHaveValidationErrorFor(x => x.Serial), Is.Not.Null);
        }
    }
}
