using Microsoft.EntityFrameworkCore;
using RiderHub.Application.Interfaces;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Messaging;

namespace RiderHub.WebApi.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IHostEnvironment env, IConfiguration configuration)
        {
            if (!env.IsEnvironment("Testing"))
            {
                services.AddDbContext<RiderHubContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

                services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
                services.AddHostedService<RabbitMQConsumer>();
            }

            return services;
        }
    }
}
