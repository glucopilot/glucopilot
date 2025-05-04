using NUnit.Framework;

namespace GlucoPilot.Data.Tests;

[TestFixture]
internal sealed class GlucoPilotDbContextTests
{
    // This is a placeholder test to ensure the test suite runs without errors.
    // This needs some thought as we can't use SQLite in memory for the tests due to 
    // foreign key constraints and the way the database is set up. Rather than limiting our
    // feature set, we will do some data integration tests in the pipeline against a real
    // MSSQL database.

    [Test]
    public void Placeholder()
    {
        Assert.Pass();
    }

    /*private SqliteConnection _connection;
    private GlucoPilotDbContext _dbContext;
    private readonly Guid _userId = Guid.NewGuid();

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
            UserId = _userId,
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
            UserId = _userId,
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
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal",
        };
        var mealIngredient = new List<MealIngredient>
        {
            new MealIngredient
            {
                Id = Guid.NewGuid(),
                MealId = meal.Id,
                IngredientId = ingredient.Id,
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
            UserId = _userId,
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
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal",
        };
        var mealIngredient = new List<MealIngredient>
        {
            new MealIngredient
            {
                Id = Guid.NewGuid(),
                MealId = meal.Id,
                IngredientId = ingredient.Id,
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
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal"
        };
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
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
            Id = Guid.NewGuid(),
            IngredientId = ingredient.Id,
            MealId = meal.Id,
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

    [Test]
    public void Can_Add_And_Retrieve_Insulin()
    {
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Insulin",
            Type = InsulinType.Bolus,
            Duration = 4.0,
            Scale = 1.0,
            PeakTime = 2.0
        };
        _dbContext.Insulins.Add(insulin);
        _dbContext.SaveChanges();
        var retrievedInsulin = _dbContext.Insulins.Find(insulin.Id);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedInsulin, Is.Not.Null);
            Assert.That(insulin, Is.EqualTo(retrievedInsulin));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Injection()
    {
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Insulin",
            Type = InsulinType.Bolus,
            Duration = 4.0,
            Scale = 1.0,
            PeakTime = 2.0
        };
        var injection = new Injection
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            InsulinId = insulin.Id,
            Units = 10.0,
            Insulin = insulin
        };
        _dbContext.Injections.Add(injection);
        _dbContext.SaveChanges();
        var retrievedInjection = _dbContext.Injections.Include(i => i.Insulin).First();
        Assert.Multiple(() =>
        {
            Assert.That(retrievedInjection, Is.Not.Null);
            Assert.That(retrievedInjection, Is.EqualTo(injection));
        });
    }

    [Test]
    public void Can_Add_And_Retrieve_Treatment([Values] TreatmentType type)
    {
        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 120.5,
            Direction = ReadingDirection.Steady,
        };
        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            Name = "Test Meal",
        };
        var insulin = new Insulin
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.UtcNow,
            Name = "Test Insulin",
            Type = InsulinType.Bolus,
            Duration = 4.0,
            Scale = 1.0,
            PeakTime = 2.0
        };
        var injection = new Injection
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            InsulinId = insulin.Id,
            Units = 10.0,
        };
        var treatment = new Treatment
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            ReadingId = reading.Id,
            MealId = (type == TreatmentType.Meal || type == TreatmentType.Correction) ? meal.Id : null,
            InjectionId = (type == TreatmentType.Meal || type == TreatmentType.Injection) ? injection.Id : null,
            Reading = reading,
            Meal = (type == TreatmentType.Meal || type == TreatmentType.Correction) ? meal : null,
            Injection = (type == TreatmentType.Meal || type == TreatmentType.Injection) ? injection : null,
        };

        _dbContext.Insulins.Add(insulin);
        _dbContext.Treatments.Add(treatment);
        _dbContext.SaveChanges();

        var retrievedTreatment = _dbContext.Treatments.Include(t => t.Reading).Include(t => t.Meal).Include(t => t.Injection).First();
        Assert.Multiple(() =>
        {
            Assert.That(retrievedTreatment, Is.Not.Null);
            Assert.That(retrievedTreatment, Is.EqualTo(treatment));
            Assert.That(retrievedTreatment.Type, Is.EqualTo(type));
        });
    }

    [Test]
    public void Throws_Error_When_Treatment_Type_Is_Invalid()
    {
        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            GlucoseLevel = 120.5,
            Direction = ReadingDirection.Steady,
        };
        var treatment = new Treatment
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Created = DateTimeOffset.UtcNow,
            ReadingId = reading.Id,
            MealId = null,
            InjectionId = null,
            Reading = reading,
            Meal = null,
            Injection = null,
        };

        _dbContext.Treatments.Add(treatment);
        _dbContext.SaveChanges();

        var retrievedTreatment = _dbContext.Treatments.Include(t => t.Reading).Include(t => t.Meal).Include(t => t.Injection).First();
        Assert.Multiple(() =>
        {
            Assert.That(retrievedTreatment, Is.Not.Null);
            Assert.That(retrievedTreatment, Is.EqualTo(treatment));
            Assert.Throws<InvalidOperationException>(() => { var type = retrievedTreatment.Type; });
        });
    }*/
}