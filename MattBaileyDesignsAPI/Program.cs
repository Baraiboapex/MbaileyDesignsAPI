using MattBaileyDesignsAPI.Controllers.Helpers.ControllerErrorHandlingHelper;
using MattBaileyDesignsAPI.Controllers.Helpers;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.Services.Auth.Interfaces;
using MattBaileyDesignsAPI.Services.Auth.JWTTokenService;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using MattBaileyDesignsAPI.Services.Communication;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorEmailSender;
using MattBaileyDesignsAPI.Services.ErrorHandling.Interfaces.ErrorLoggers;
using MattBaileyDesignsAPI.Services.ErrorHandling;
using MbaileyDesignsPersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using MBaileyDesignsDomain;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "MbaileyDesignsAPI", Version = "v1" });
        var fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        c.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
        c.AddSecurityRequirement(
            new OpenApiSecurityRequirement { {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }, new string[] { }
            }
        });
    });

    builder.Services.AddIdentityService(builder.Configuration);
    builder.Services.AddSingleton<ConfigurationBuilder>();
    builder.Services.AddDbContext<PostgresDataContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped(typeof(IDBRepository<>), typeof(PosgresDBRepository<>));

    builder.Services.AddTransient<IEmailerService, GmailEmailerService>();
    builder.Services.AddSingleton<ITokenService, JWTTokenService>();
    builder.Services.AddTransient<IErrorLoggingHandler>((service) =>
    {
        var getService = service.GetRequiredService<IDBRepository<Error>>();
        return new PostGresErrorHandler(getService);
    });
    builder.Services.AddTransient<IErrorEmailSender, GmailErrorSender>();
    builder.Services.AddTransient<IControllerErrorHandlingHelper, CompleteControllerErrorHandlingHelper>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MbaileyDesigns API V1");
        });
    }

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<PostgresDataContext>();
            Console.WriteLine("DbContext resolved successfully.");
            dbContext.Database.Migrate();
            await Seed.SeedData(dbContext);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while migrating or initializing the database." + ex.ToString());
        }
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"Request path: {context.Request.Path}, Authorization header: {context.Request.Headers["Authorization"]}");
        await next.Invoke();
    });

    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature.Error;

            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, "An unhandled exception has occurred.");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred.");
        });
    });

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    throw new Exception("ERROR: " + ex.ToString());
}
