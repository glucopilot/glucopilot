using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Injections.NewInjection;
using NUnit.Framework;
using System;

namespace GlucoPilot.Api.Tests.Endpoints.Injections
{
    [TestFixture]
    public class NewInjectionRequestValidatorTests
    {
        private NewInjectionRequest.NewInjectionRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new NewInjectionRequest.NewInjectionRequestValidator();
        }

        [Test]
        public void Should_Have_Error_When_InsulinId_Is_Empty()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.Empty,
                Units = 10,
                Created = DateTimeOffset.UtcNow
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.InsulinId)
                .WithErrorMessage("InsulinId is required.");
        }

        [Test]
        public void Should_Not_Have_Error_When_InsulinId_Is_Valid()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 10,
                Created = DateTimeOffset.UtcNow
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.InsulinId);
        }

        [Test]
        public void Should_Have_Error_When_Units_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 0,
                Created = DateTimeOffset.UtcNow
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Units)
                .WithErrorMessage("Units must be greater than 0.");
        }

        [Test]
        public void Should_Not_Have_Error_When_Units_Is_Greater_Than_Zero()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 5,
                Created = DateTimeOffset.UtcNow
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Units);
        }

        [Test]
        public void Should_Not_Have_Error_When_Created_Is_Valid_Date()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 5,
                Created = DateTimeOffset.UtcNow
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Created);
        }

        [Test]
        public void Should_Have_Error_When_Created_Is_Invalid_Date()
        {
            var model = new NewInjectionRequest
            {
                InsulinId = Guid.NewGuid(),
                Units = 5,
                Created = default
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Created)
                .WithErrorMessage("Created date is required.");
        }
    }
}