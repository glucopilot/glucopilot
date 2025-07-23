using System.Text.Json.Serialization;
using Asp.Versioning;
using FluentValidation;
using GlucoPilot.Api.Endpoints;
using GlucoPilot.Api.Middleware;
using GlucoPilot.Api.Models;
using GlucoPilot.Api.Swagger;
using GlucoPilot.Data;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.Mail;
using GlucoPilot.Sync.LibreLink;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

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
    opt.CustomSchemaIds(type =>
    {
        return type.FullName.Replace("GlucoPilot.Api.Endpoints.", "")
            .Replace("GlucoPilot.Identity.Endpoints.", "")
            .Replace("GlucoPilot.Api.Models.", "")
            .Replace("GlucoPilot.Identity.Models.", "")
            .Replace(".", "_");
    });
    opt.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "bearer"
    });
    opt.OperationFilter<SecurityRequirementsOperationFilter>();
    opt.SchemaFilter<XEnumNamesSchemaFilter>();
});

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));

builder.Services.AddHealthChecks().AddDatabaseHealthChecks();
builder.Services.AddData(builder.Configuration.GetSection("Data").Bind);
builder.Services.AddIdentity(builder.Configuration.GetSection("Identity").Bind);

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddScoped<GlucoPilotDbInitializer>();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddLibreLinkClient(builder.Configuration.GetSection("LibreLink").Bind);

builder.Services.AddMail(builder.Configuration.GetSection("Mail").Bind);

builder.Services.AddHostedService<SyncService>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSerilog(dispose: true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseIdentity();

app.UseHealthChecks("/health");

app.MapIdentityEndpoints();
app.MapGlucoPilotEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<GlucoPilotDbInitializer>();
    await dbInitializer.InitialiseDbAsync(app.Lifetime.ApplicationStopping);
}

await app.RunAsync();