using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiderHub.Infrastructure.Context;
using Moq;
using RiderHub.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace RiderHub.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing"); // Set the environment to Testing

            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RiderHubContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove RabbitMQ services
                var rabbitMqPublisherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessagePublisher));
                if (rabbitMqPublisherDescriptor != null)
                {
                    services.Remove(rabbitMqPublisherDescriptor);
                }

                var rabbitMqConsumerDescriptor = services.SingleOrDefault(d => d.ImplementationType == typeof(RiderHub.Infrastructure.Messaging.RabbitMQConsumer));
                if (rabbitMqConsumerDescriptor != null)
                {
                    services.Remove(rabbitMqConsumerDescriptor);
                }

                // Add DbContext using an in-memory database for testing.
                services.AddDbContext<RiderHubContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Add mocked RabbitMQ services
                services.AddSingleton(Mock.Of<IMessagePublisher>());
                services.AddSingleton(Mock.Of<IHostedService>()); // Mock RabbitMQConsumer

                // Add dummy authentication for testing
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database contexts
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<RiderHubContext>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    // Clear the database before each test run
                    db.Motorcycles.RemoveRange(db.Motorcycles);
                    db.DeliveryDrivers.RemoveRange(db.DeliveryDrivers);
                    db.Rentals.RemoveRange(db.Rentals);
                    db.RentalPlans.RemoveRange(db.RentalPlans);
                    db.Notifications.RemoveRange(db.Notifications);
                    db.SaveChanges();
                }
            });
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "TestUser"), new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.Role, "DeliveryDriver") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
