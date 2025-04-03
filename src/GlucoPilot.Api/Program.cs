using GlucoPilot.Data;
using GlucoPilot.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning();

builder.Services.AddHealthChecks().AddDatabaseHealthChecks();
builder.Services.AddData(builder.Configuration.GetSection("Data").Bind);
builder.Services.AddIdentity(builder.Configuration.GetSection("Identity").Bind);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIdentity();

app.UseHealthChecks("/health");

app.MapIdentityEndpoints();

await app.RunAsync();