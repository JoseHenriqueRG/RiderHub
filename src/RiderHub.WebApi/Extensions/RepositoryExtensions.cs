using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Repositories;

namespace RiderHub.WebApi.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
            services.AddScoped<IRentalRepository, RentalRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IDeliveryDriverRepository, DeliveryDriverRepository>();
            services.AddScoped<IRentalPlanRepository, RentalPlanRepository>();

            return services;
        }
    }
}
