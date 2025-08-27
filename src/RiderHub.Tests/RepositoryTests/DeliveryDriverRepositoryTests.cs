using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Enums;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Repositories;
using Xunit;

namespace RiderHub.Tests.RepositoryTests
{
    public class DeliveryDriverRepositoryTests
    {
        private RiderHubContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RiderHubContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RiderHubContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddDeliveryDriver()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "11111111111111",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url"
            };

            // Act
            await repository.AddAsync(driver);

            // Assert
            var addedDriver = await context.DeliveryDrivers.FirstOrDefaultAsync(d => d.CNPJ == "11111111111111");
            Assert.NotNull(addedDriver);
            Assert.Equal("11111111111111", addedDriver.CNPJ);
        }

        [Fact]
        public async Task GetByCNPJAsync_ShouldReturnDriver_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "22222222222222",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "987654321",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url"
            };
            await repository.AddAsync(driver);

            // Act
            var foundDriver = await repository.GetByCNPJAsync("22222222222222");

            // Assert
            Assert.NotNull(foundDriver);
            Assert.Equal("22222222222222", foundDriver.CNPJ);
        }

        [Fact]
        public async Task GetByCNPJAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);

            // Act
            var foundDriver = await repository.GetByCNPJAsync("99999999999999");

            // Assert
            Assert.Null(foundDriver);
        }

        [Fact]
        public async Task GetByDriverLicenseNumberAsync_ShouldReturnDriver_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "33333333333333",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "111222333",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url"
            };
            await repository.AddAsync(driver);

            // Act
            var foundDriver = await repository.GetByDriverLicenseNumberAsync("111222333");

            // Assert
            Assert.NotNull(foundDriver);
            Assert.Equal("111222333", foundDriver.DriverLicenseNumber);
        }

        [Fact]
        public async Task GetByDriverLicenseNumberAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);

            // Act
            var foundDriver = await repository.GetByDriverLicenseNumberAsync("999999999");

            // Assert
            Assert.Null(foundDriver);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDeliveryDriver()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "44444444444444",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "444555666",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "old_image_url"
            };
            await repository.AddAsync(driver);

            driver.DriverLicenseImage = "new_image_url";

            // Act
            await repository.UpdateAsync(driver);

            // Assert
            var updatedDriver = await context.DeliveryDrivers.FindAsync(driver.Id);
            Assert.NotNull(updatedDriver);
            Assert.Equal("new_image_url", updatedDriver.DriverLicenseImage);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDriver_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);
            var driver = new DeliveryDriver
            {
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CNPJ = "55555555555555",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "777888999",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url"
            };
            await repository.AddAsync(driver);

            // Act
            var foundDriver = await repository.GetByIdAsync(driver.Id);

            // Assert
            Assert.NotNull(foundDriver);
            Assert.Equal(driver.CNPJ, foundDriver.CNPJ);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new DeliveryDriverRepository(context);

            // Act
            var foundDriver = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(foundDriver);
        }
    }
}
