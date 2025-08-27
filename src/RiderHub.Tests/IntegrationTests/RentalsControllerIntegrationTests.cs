using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RiderHub.Domain.Entities;
using RiderHub.Infrastructure.Context;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using RiderHub.WebApi;
using RiderHub.Domain.Enums;
using RiderHub.Application.Dtos;

namespace RiderHub.Tests.IntegrationTests
{
    public class RentalsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public RentalsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<DeliveryDriver> CreateTestDeliveryDriver(RiderHubContext context)
        {
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test.driver@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "11111111111111",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url",
                Role = UserRoleEnum.DeliveryDriver
            };
            context.DeliveryDrivers.Add(driver);
            await context.SaveChangesAsync();
            return driver;
        }

        private async Task<Motorcycle> CreateTestMotorcycle(RiderHubContext context)
        {
            var motorcycle = new Motorcycle { Year = 2023, Model = "TestMoto", LicensePlate = "TEST123" };
            context.Motorcycles.Add(motorcycle);
            await context.SaveChangesAsync();
            return motorcycle;
        }

        private async Task<RentalPlan> CreateTestRentalPlan(RiderHubContext context, int days, decimal dailyRate)
        {
            var plan = new RentalPlan { Days = days, DailyRate = dailyRate };
            context.RentalPlans.Add(plan);
            await context.SaveChangesAsync();
            return plan;
        }

        [Fact]
        public async Task CreateRental_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RiderHubContext>();
                var driver = await CreateTestDeliveryDriver(context);
                var motorcycle = await CreateTestMotorcycle(context);
                await CreateTestRentalPlan(context, 7, 30.00m);

                var createDto = new { DeliveryDriverId = driver.Id, MotorcycleId = motorcycle.Id, PlanDays = 7 };

                // Act
                var response = await _client.PostAsJsonAsync("/api/Rentals", createDto);

                // Assert
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }
        }

        [Fact]
        public async Task CalculateRentalCost_ShouldReturnOk_OnTimeReturn()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RiderHubContext>();
                var driver = await CreateTestDeliveryDriver(context);
                var motorcycle = await CreateTestMotorcycle(context);
                await CreateTestRentalPlan(context, 7, 30.00m);

                var createDto = new { DeliveryDriverId = driver.Id, MotorcycleId = motorcycle.Id, PlanDays = 7 };
                var createResponse = await _client.PostAsJsonAsync("/api/Rentals", createDto);
                var createdRental = await createResponse.Content.ReadFromJsonAsync<Rental>();

                // Ensure all required properties are set for the deserialized object
                createdRental.DeliveryDriver = driver;
                createdRental.Motorcycle = motorcycle;

                var returnDto = new { RentalId = createdRental.Id, ReturnDate = createdRental.ExpectedReturnDate };

                // Act
                var response = await _client.PutAsJsonAsync("/api/Rentals/calculate-cost", returnDto);

                // Assert
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
                }
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = await response.Content.ReadFromJsonAsync<CalculateCostResponseDto>();
                Assert.Equal(7 * 30.00m, result.TotalCost);
            }
        }

        [Fact]
        public async Task CalculateRentalCost_ShouldReturnOk_EarlyReturn_7DaysPlan()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RiderHubContext>();
                var driver = await CreateTestDeliveryDriver(context);
                var motorcycle = await CreateTestMotorcycle(context);
                await CreateTestRentalPlan(context, 7, 30.00m);

                var createDto = new { DeliveryDriverId = driver.Id, MotorcycleId = motorcycle.Id, PlanDays = 7 };
                var createResponse = await _client.PostAsJsonAsync("/api/Rentals", createDto);
                var createdRental = await createResponse.Content.ReadFromJsonAsync<Rental>();

                // Ensure all required properties are set for the deserialized object
                createdRental.DeliveryDriver = driver;
                createdRental.Motorcycle = motorcycle;

                var returnDto = new { RentalId = createdRental.Id, ReturnDate = createdRental.ExpectedReturnDate.AddDays(-3) }; // 3 days early

                // Act
                var response = await _client.PutAsJsonAsync("/api/Rentals/calculate-cost", returnDto);

                // Assert
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
                }
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = await response.Content.ReadFromJsonAsync<CalculateCostResponseDto>();
                var actualDaysRented = (decimal)(createdRental.ExpectedReturnDate.AddDays(-3) - createdRental.StartDate).TotalDays;
                var remainingDays = (decimal)(createdRental.ExpectedReturnDate - createdRental.ExpectedReturnDate.AddDays(-3)).TotalDays;
                var expectedCost = (actualDaysRented * 30.00m) + (remainingDays * 30.00m * 0.20m);
                Assert.Equal(expectedCost, result.TotalCost);
            }
        }

        [Fact]
        public async Task CalculateRentalCost_ShouldReturnOk_LateReturn()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RiderHubContext>();
                var driver = await CreateTestDeliveryDriver(context);
                var motorcycle = await CreateTestMotorcycle(context);
                await CreateTestRentalPlan(context, 7, 30.00m);

                var createDto = new { DeliveryDriverId = driver.Id, MotorcycleId = motorcycle.Id, PlanDays = 7 };
                var createResponse = await _client.PostAsJsonAsync("/api/Rentals", createDto);
                var createdRental = await createResponse.Content.ReadFromJsonAsync<Rental>();

                // Ensure all required properties are set for the deserialized object
                createdRental.DeliveryDriver = driver;
                createdRental.Motorcycle = motorcycle;

                var returnDto = new { RentalId = createdRental.Id, ReturnDate = createdRental.ExpectedReturnDate.AddDays(2) }; // 2 days late

                // Act
                var response = await _client.PutAsJsonAsync("/api/Rentals/calculate-cost", returnDto);

                // Assert
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
                }
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = await response.Content.ReadFromJsonAsync<CalculateCostResponseDto>();
                var expectedCost = (7 * 30.00m) + (2 * 50.00m);
                Assert.Equal(expectedCost, result.TotalCost);
            }
        }
    }
}
