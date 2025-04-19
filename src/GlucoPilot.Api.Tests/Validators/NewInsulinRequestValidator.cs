using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Insulins.NewInsulin;
using GlucoPilot.Data.Enums;
using NUnit.Framework;

namespace GlucoPilot.Tests.Validators
{
    [TestFixture]
    public class NewInsulinValidatorTests
    {
        private NewInsulinRequest.NewInsulinRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new NewInsulinRequest.NewInsulinRequestValidator();
        }

        [Test]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new NewInsulinRequest { Name = string.Empty, Type = InsulinType.Bolus };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("NAME_REQUIRED");
        }

        [Test]
        public void Should_Have_Error_When_Type_Is_Invalid()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = (InsulinType)(-1) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Type)
                .WithErrorMessage("INSULIN_TYPE_INVALID");
        }

        [Test]
        public void Should_Have_Error_When_Duration_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, Duration = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Duration)
                .WithErrorMessage("DURATION_GREATER_THAN_ZERO");
        }

        [Test]
        public void Should_Not_Have_Error_When_Duration_Is_Null()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, Duration = null };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Duration);
        }

        [Test]
        public void Should_Have_Error_When_Scale_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, Scale = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Scale)
                .WithErrorMessage("SCALE_GREATER_THAN_ZERO");
        }

        [Test]
        public void Should_Not_Have_Error_When_Scale_Is_Null()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, Scale = null };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Scale);
        }

        [Test]
        public void Should_Have_Error_When_PeakTime_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, PeakTime = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PeakTime)
                .WithErrorMessage("PEAKTIME_GREATER_THAN_ZERO");
        }

        [Test]
        public void Should_Not_Have_Error_When_PeakTime_Is_Null()
        {
            var model = new NewInsulinRequest { Name = "ValidName", Type = InsulinType.Bolus, PeakTime = null };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PeakTime);
        }
    }
}