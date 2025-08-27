using RiderHub.Application.Interfaces;
using RiderHub.Application.Services;

namespace RiderHub.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IMotorcycleService, MotorcycleService>();
            services.AddScoped<IDeliveryDriverService, DeliveryDriverService>();
            services.AddScoped<IRentalService, RentalService>();

            return services;
        }
    }

}
