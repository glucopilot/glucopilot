using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Sensors.List;
using GlucoPilot.Api.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators
{
    [TestFixture]
    class ListSensorsRequestValidatorTests
    {
        private readonly ListSensorsRequest.ListSensorsValidator _validator;

        public ListSensorsRequestValidatorTests()
        {
            var apiSettings = Options.Create(new ApiSettings
            {
                MaxPageSize = 25
            });
            _validator = new ListSensorsRequest.ListSensorsValidator(apiSettings);
        }

        [Test]
        public void Should_Have_Error_When_Page_Is_Less_Than_Zero()
        {
            var request = new ListSensorsRequest
            {
                Page = -1,
                PageSize = 10
            };

            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Page);
        }

        [Test]
        public void Should_Have_Error_When_PageSize_Is_Less_Than_One()
        {
            var request = new ListSensorsRequest
            {
                Page = 1,
                PageSize = 0
            };

            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.PageSize);
        }

        [Test]
        public void Should_Have_Error_When_PageSize_Exceeds_MaxPageSize()
        {
            var request = new ListSensorsRequest
            {
                Page = 1,
                PageSize = 26
            };

            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.PageSize);
        }

        [Test]
        public void Should_Not_Have_Error_For_Valid_Request()
        {
            var request = new ListSensorsRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
