using Microsoft.EntityFrameworkCore;
using RiderHub.Application.Interfaces;
using RiderHub.Application.Services;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Messaging;
using RiderHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder; // Add this using directive
using Microsoft.Extensions.Logging;
using RiderHub.WebApi.Middlewares;
using RiderHub.WebApi.Filters;
using RiderHub.WebApi.Extensions;

namespace RiderHub.WebApi
{
    public class Program // Make the Program class public
    {
        public static void Main(string[] args) // Make Main method public
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
               .AddInfrastructure(builder.Environment, builder.Configuration)
               .AddRepositories()
               .AddApplicationServices();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SchemaFilter<EnumSchemaFilter>();
            });

            var app = builder.Build();

            app.UseExceptionHandling();

            // Apply migrations on startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<RiderHubContext>();
                    if (context.Database.IsRelational())
                    {
                        context.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}