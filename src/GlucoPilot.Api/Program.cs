using System.Text.Json.Serialization;
using Asp.Versioning;
using GlucoPilot.Api.Middleware;
using GlucoPilot.Api.Swagger;
using GlucoPilot.Data;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .EnableApiVersionBinding();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "bearer"
    });
    opt.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddHealthChecks().AddDatabaseHealthChecks();
builder.Services.AddData(builder.Configuration.GetSection("Data").Bind);
builder.Services.AddIdentity(builder.Configuration.GetSection("Identity").Bind);

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddScoped<GlucoPilotDbInitializer>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseIdentity();

app.UseHealthChecks("/health");


app.MapIdentityEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<GlucoPilotDbInitializer>();
    await dbInitializer.InitialiseDbAsync(app.Lifetime.ApplicationStopping);
}

await app.RunAsync();