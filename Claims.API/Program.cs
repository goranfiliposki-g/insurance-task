using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Application.Validators;
using Claims.Infrastructure;
using FluentValidation;

namespace Claims.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers(o => o.Filters.Add<Claims.API.Filters.ValidationExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Claims API", Version = "v1", Description = "Insurance Claims and Covers API" });
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "Claims.API.xml");
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        builder.Services.AddClaimsInfrastructure();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateCoverDtoValidator>();
        builder.Services.AddSingleton<IPremiumPolicy, DefaultPremiumPolicy>();
        builder.Services.AddSingleton<IPremiumCalculator, PremiumCalculator>();
        builder.Services.AddScoped<IClaimService, ClaimService>();
        builder.Services.AddScoped<ICoverService, CoverService>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        app.Run();
    }
}

