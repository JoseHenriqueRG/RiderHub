using RiderHub.Domain.Entities;
using RiderHub.Domain.Enums;
using RiderHub.WebApi;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace RiderHub.Tests.IntegrationTests
{
    public class DeliveryDriversControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DeliveryDriversControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private MultipartFormDataContent CreateMultipartFormDataContent(string name, string email, string password, string cnpj, string dateOfBirth, string driverLicenseNumber, string driverLicenseType, string fileName, string fileContent, string contentType)
        {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "Name");
            form.Add(new StringContent(email), "Email");
            form.Add(new StringContent(password), "Password");
            form.Add(new StringContent(cnpj), "CNPJ");
            form.Add(new StringContent(dateOfBirth), "DateOfBirth");
            form.Add(new StringContent(driverLicenseNumber), "DriverLicenseNumber");
            form.Add(new StringContent(driverLicenseType), "DriverLicenseType");

            var fileStreamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)));
            fileStreamContent.Headers.Add("Content-Type", contentType);
            form.Add(fileStreamContent, "DriverLicenseImage", fileName);

            return form;
        }

        private string GenerateValidCnpj()
        {
            Random random = new Random();
            string cnpj = "";
            for (int i = 0; i < 14; i++)
            {
                cnpj += random.Next(0, 10).ToString();
            }
            return cnpj;
        }

        private string GenerateUniqueDriverLicenseNumber()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 9).ToUpper();
        }

        [Fact]
        public async Task RegisterDeliveryDriver_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            var uniqueCnpj = GenerateValidCnpj();
            var uniqueDriverLicenseNumber = GenerateUniqueDriverLicenseNumber();
            var formContent = CreateMultipartFormDataContent(
                "Integration Test Driver",
                "integration@example.com",
                "Password123",
                uniqueCnpj,
                new DateTime(1990, 1, 1).ToString("o"), // ISO 8601 format
                uniqueDriverLicenseNumber,
                "A",
                "cnh.png",
                "image_data",
                "image/png"
            );

            // Act
            var response = await _client.PostAsync("/api/DeliveryDrivers/register", formContent);

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
        public async Task UpdateCnhImage_ShouldReturnNoContent_WhenValidData()
        {
            // Arrange - Register a driver first
            var uniqueCnpj = GenerateValidCnpj();
            var uniqueDriverLicenseNumber = GenerateUniqueDriverLicenseNumber();
            var registerFormContent = CreateMultipartFormDataContent(
                "Update Test Driver",
                "update@example.com",
                "Password123",
                uniqueCnpj,
                new DateTime(1990, 1, 1).ToString("o"), // ISO 8601 format
                uniqueDriverLicenseNumber,
                "A",
                "old_cnh.png",
                "old_image_data",
                "image/png"
            );
            var registerResponse = await _client.PostAsync("/api/DeliveryDrivers/register", registerFormContent);
            if (!registerResponse.IsSuccessStatusCode)
            {
                var errorContent = await registerResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to register driver for update test: {registerResponse.StatusCode} - {errorContent}");
            }
            var registeredDriver = await registerResponse.Content.ReadFromJsonAsync<DeliveryDriver>();

            // Ensure all required properties are set for the deserialized object
            registeredDriver.Name = registeredDriver.Name ?? "Default Name";
            registeredDriver.Email = registeredDriver.Email ?? "default@example.com";
            registeredDriver.PasswordHash = registeredDriver.PasswordHash ?? "default_hash";
            registeredDriver.CNPJ = registeredDriver.CNPJ ?? "00000000000000";
            registeredDriver.DriverLicenseNumber = registeredDriver.DriverLicenseNumber ?? "000000000";
            registeredDriver.DriverLicenseType = registeredDriver.DriverLicenseType;
            registeredDriver.DriverLicenseImage = registeredDriver.DriverLicenseImage ?? "default_image.png";
            registeredDriver.Role = registeredDriver.Role; // Assuming Role is already set during registration

            var updateFormContent = new MultipartFormDataContent();
            updateFormContent.Add(new StringContent(registeredDriver.Id.ToString()), "DeliveryDriverId");
            var fileStreamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("new_image_data")));
            fileStreamContent.Headers.Add("Content-Type", "image/bmp");
            updateFormContent.Add(fileStreamContent, "CnhImageFile", "new_cnh.bmp");

            // Act
            var response = await _client.PutAsync("/api/DeliveryDrivers/cnh-image", updateFormContent);

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