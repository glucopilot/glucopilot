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
            Direction = ReadingDirection.Steady,
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
            Name = "Ingredient 1",
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams,
        };
        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal",
        };
        var mealIngredient = new List<MealIngredient>
        {
            new MealIngredient
            {
                MealIngredientId = Guid.NewGuid(),
                Quantity = 1,
                Ingredient = ingredient,
                Meal = meal,
            }
        };
        meal.MealIngredients = mealIngredient;
        ingredient.Meals = mealIngredient;

        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();

        var retrievedIngredient = _dbContext.Ingredients.Find(ingredient.Id);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedIngredient, Is.Not.Null);
            Assert.That(ingredient, Is.EqualTo(retrievedIngredient));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Meal()
    {
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Ingredient 1",
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams,
        };
        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal",
        };
        var mealIngredient = new List<MealIngredient>
        {
            new MealIngredient
            {
                MealIngredientId = Guid.NewGuid(),
                Quantity = 1,
                Ingredient = ingredient,
                Meal = meal,
            }
        };
        meal.MealIngredients = mealIngredient;
        ingredient.Meals = mealIngredient;

        _dbContext.Meals.Add(meal);
        _dbContext.SaveChanges();

        var retrievedMeal = _dbContext.Meals
            .Include(m => m.MealIngredients)
            .FirstOrDefault(m => m.Id == meal.Id);

        Assert.Multiple(() =>
        {
            Assert.That(retrievedMeal, Is.Not.Null);
            Assert.That(retrievedMeal, Is.EqualTo(meal));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Meal_Ingredient()
    {
        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal"
        };
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Test Ingredient",
            Carbs = 10,
            Protein = 5,
            Fat = 2,
            Calories = 100,
            Uom = UnitOfMeasurement.Grams
        };
        var mealIngredient = new MealIngredient
        {
            MealIngredientId = Guid.NewGuid(),
            Meal = meal,
            Ingredient = ingredient,
            Quantity = 1,
        };

        _dbContext.MealIngredients.Add(mealIngredient);
        _dbContext.SaveChanges();

        Assert.That(_dbContext.MealIngredients.Count(), Is.EqualTo(1));
        var retrievedMealIngredient = _dbContext.MealIngredients.Include(mi => mi.Meal).Include(mi => mi.Ingredient).First();
        Assert.That(mealIngredient, Is.Not.Null);
        Assert.That(mealIngredient.Meal, Is.EqualTo(meal));
        Assert.That(mealIngredient.Ingredient, Is.EqualTo(ingredient));
    }
}