using RiderHub.Domain.Entities;
using RiderHub.WebApi;
using System.Net;
using System.Net.Http.Json;

namespace RiderHub.Tests.IntegrationTests
{
    public class MotorcyclesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public MotorcyclesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateMotorcycle_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            var uniqueLicensePlate = Guid.NewGuid().ToString().Substring(0, 7).ToUpper();
            var createDto = new { Year = 2023, Model = "TestModel", LicensePlate = uniqueLicensePlate };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Motorcycles", createDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetMotorcycles_ShouldReturnOk_WhenCalled()
        {
            // Arrange 
            var uniqueLicensePlate = Guid.NewGuid().ToString().Substring(0, 7).ToUpper();
            var createDto = new { Year = 2023, Model = "GetModel", LicensePlate = uniqueLicensePlate };
            await _client.PostAsJsonAsync("/api/Motorcycles", createDto);

            // Act
            var response = await _client.GetAsync("/api/Motorcycles");

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMotorcycleLicensePlate_ShouldReturnNoContent_WhenValidData()
        {
            // Arrange
            var uniqueLicensePlate = Guid.NewGuid().ToString().Substring(0, 7).ToUpper();
            var createDto = new { Year = 2023, Model = "UpdateModel", LicensePlate = uniqueLicensePlate };
            var createResponse = await _client.PostAsJsonAsync("/api/Motorcycles", createDto);
            var createdMotorcycle = await createResponse.Content.ReadFromJsonAsync<Motorcycle>();

            var updateDto = new { Id = createdMotorcycle.Id, LicensePlate = Guid.NewGuid().ToString().Substring(0, 7).ToUpper() };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Motorcycles/license-plate", updateDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMotorcycle_ShouldReturnNoContent_WhenNoRentalsExist()
        {
            // Arrange
            var uniqueLicensePlate = Guid.NewGuid().ToString().Substring(0, 7).ToUpper();
            var createDto = new { Year = 2023, Model = "DeleteModel", LicensePlate = uniqueLicensePlate };
            var createResponse = await _client.PostAsJsonAsync("/api/Motorcycles", createDto);
            var createdMotorcycle = await createResponse.Content.ReadFromJsonAsync<Motorcycle>();

            // Act
            var response = await _client.DeleteAsync($"/api/Motorcycles/{createdMotorcycle.Id}");

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"API call failed with status code {response.StatusCode} and content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}