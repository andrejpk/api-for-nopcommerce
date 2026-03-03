using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Nop.Plugin.Api.Controllers;
using NUnit.Framework;
using Swashbuckle.AspNetCore.Swagger;

namespace Nop.Plugin.Api.Tests;

[TestFixture]
public class SwaggerSchemaGenerationTests
{
    [Test]
    public void GetSwagger_GeneratesSchemaWithoutErrors()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddMvcCore()
            .AddNewtonsoftJson()
            .AddApiExplorer()
            .AddApplicationPart(typeof(OrdersController).Assembly);

        builder.Services.Configure<MvcNewtonsoftJsonOptions>(options =>
        {
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Nop API", Version = "v1" });
            options.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
        });
        builder.Services.AddSwaggerGenNewtonsoftSupport();

        var app = builder.Build();
        var swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();

        Assert.DoesNotThrow(() => swaggerProvider.GetSwagger("v1"));
    }
}
