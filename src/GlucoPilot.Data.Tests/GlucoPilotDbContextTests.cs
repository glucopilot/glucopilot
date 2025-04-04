using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GlucoPilot.Data.Tests;

[TestFixture]
internal sealed class GlucoPilotDbContextTests
{
    private SqliteConnection _connection;
    private GlucoPilotDbContext _dbContext;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<GlucoPilotDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new GlucoPilotDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    [Test]
    public void Context_Can_Be_Created()
    {
        Assert.That(_dbContext, Is.Not.Null);
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Dispose();
        _dbContext?.Dispose();
    }

    [Test]
    public void Can_Add_And_Retrieve_Reading()
    {
        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 120.5,
            Direction = ReadingDirection.Steady
        };

        _dbContext.Readings.Add(reading);
        _dbContext.SaveChanges();

        var retrievedReading = _dbContext.Readings.Find(reading.Id);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedReading, Is.Not.Null);
            Assert.That(reading.GlucoseLevel, Is.EqualTo(retrievedReading.GlucoseLevel));
            Assert.That(reading.Direction, Is.EqualTo(retrievedReading.Direction));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Ingredient()
    {
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Sugar",
            Carbs = 100,
            Protein = 0,
            Fat = 0,
            Calories = 400,
            Uom = UnitOfMeasurement.grams
        };

        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();

        var retrievedIngredient = _dbContext.Ingredients.Find(ingredient.Id);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedIngredient, Is.Not.Null);
            Assert.That(ingredient.Name, Is.EqualTo(retrievedIngredient.Name));
            Assert.That(ingredient.Carbs, Is.EqualTo(retrievedIngredient.Carbs));
            Assert.That(ingredient.Protein, Is.EqualTo(retrievedIngredient.Protein));
            Assert.That(ingredient.Fat, Is.EqualTo(retrievedIngredient.Fat));
            Assert.That(ingredient.Calories, Is.EqualTo(retrievedIngredient.Calories));
            Assert.That(ingredient.Uom, Is.EqualTo(retrievedIngredient.Uom));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Meal()
    {
        var ingredient1 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Sugar",
            Carbs = 100,
            Protein = 0,
            Fat = 0,
            Calories = 400,
            Uom = UnitOfMeasurement.grams
        };

        var ingredient2 = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Flour",
            Carbs = 75,
            Protein = 10,
            Fat = 1,
            Calories = 300,
            Uom = UnitOfMeasurement.grams
        };

        var ingredients = new List<Ingredient> { ingredient1, ingredient2 };

        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Ingredients = ingredients
        };

        _dbContext.Meals.Add(meal);
        _dbContext.SaveChanges();

        var retrievedMeal = _dbContext.Meals
            .Include(m => m.Ingredients)
            .FirstOrDefault(m => m.Id == meal.Id);

        Assert.Multiple(() =>
        {
            Assert.That(retrievedMeal, Is.Not.Null);
            Assert.That(retrievedMeal.Ingredients.Count, Is.EqualTo(2));
            Assert.That(retrievedMeal.Ingredients, Is.EquivalentTo(ingredients));
        });
    }
}