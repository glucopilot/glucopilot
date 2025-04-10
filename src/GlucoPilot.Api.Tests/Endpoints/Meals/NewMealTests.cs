using NUnit.Framework;
using Moq;
using GlucoPilot.Api.Endpoints.Meals.NewMeal;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using Microsoft.EntityFrameworkCore;

[TestFixture]
public class Endpoint_Tests
{
    private Mock<ICurrentUser> _currentUserMock;
    private Mock<IRepository<Meal>> _repositoryMock;

    [SetUp]
    public void SetUp()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _repositoryMock = new Mock<IRepository<Meal>>();
    }
    
    [Test]
    public async Task HandleAsync_Should_Return_Created_When_Meal_Is_Successfully_Created()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        var request = new NewMealRequest { Name = "Test Meal", MealIngredients = new List<MealIngredient>() };

        _repositoryMock.Setup(x => x.Add(It.IsAny<Meal>()));

        var result = await Endpoint.HandleAsync(request, _currentUserMock.Object, _repositoryMock.Object);

        Assert.That(result.Result, Is.InstanceOf<Created<NewMealResponse>>());
        var createdResult = result.Result as Created<NewMealResponse>;
        Assert.That(createdResult?.Value.Name, Is.EqualTo("Test Meal"));
    }
}