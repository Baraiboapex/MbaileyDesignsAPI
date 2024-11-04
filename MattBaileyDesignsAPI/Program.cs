using MattBaileyDesignsAPI.DbRepository;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MBaileyDesignsDomain;
using MBaileyDesignsDomain.EnvironmentHelpers;
using MbaileyDesignsPersistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Resources;

var builder = WebApplication.CreateBuilder(args);
var connectionString = new ResourceManager("MattBaileyDesignsAPI.Properties.Resources", Assembly.GetExecutingAssembly()).GetString("ConnectionString");
// Add services to the container.

//---global project settings from resources.settings file-----
try
{
    
    //---global project settings from resources.settings file-----

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<PostgresDataContext>();
    builder.Services.AddSingleton<EnvironmentVars>();
    builder.Services.AddSingleton<IDBRepository<BlogPost>>();
    builder.Services.AddSingleton<IDBRepository<AboutPost>>();
    builder.Services.AddSingleton<IDBRepository<Project>>();
    builder.Services.AddSingleton<IDBRepository<User>>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    if (CAN_AUTO_MIGRATE_DB)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<DbContext>();
            bool dbCreated = dbContext.Database.EnsureCreated();

            if (dbCreated)
            {
                dbContext.Database.Migrate();
            }
            else
            {
                throw new Exception("Failed to create database");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    app.Run();
}
catch(Exception ex)
{
    throw new Exception("ERROR: "+ex.ToString());
}

