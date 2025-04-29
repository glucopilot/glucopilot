﻿using System;
using FluentValidation.TestHelper;
using GlucoPilot.Api.Endpoints.Injections.NewInjection;
using NUnit.Framework;

namespace GlucoPilot.Api.Tests.Validators;

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
            .WithErrorMessage("INSULIN_ID_INVALID");
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
            .WithErrorMessage("UNITS_GREATER_THAN_ZERO");
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
            .WithErrorMessage("CREATED_DATE_REQUIRED");
    }
}