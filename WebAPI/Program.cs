using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using WebAPI.Models;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder
    .EnableLowerCamelCase()
    .EntitySet<Entity>("Entities");

builder.Services.AddControllers().AddOData(
    options =>
    options
        .EnableQueryFeatures()
        .AddRouteComponents("odata", modelBuilder.GetEdmModel()
        ));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<EntityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled for POC purpose
// app.UseHttpsRedirection();
// app.UseAuthorization();

app.MapControllers();

app.Run();
