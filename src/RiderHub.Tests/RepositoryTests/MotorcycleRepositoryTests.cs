using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Repositories;
using Xunit;
using RiderHub.Domain.Enums;
using RiderHub.Domain.Exceptions;

namespace RiderHub.Tests.RepositoryTests
{
    public class MotorcycleRepositoryTests
    {
        private RiderHubContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RiderHubContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RiderHubContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddMotorcycle()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Year = 2023, Model = "Model Y", LicensePlate = "TEST1234" };

            // Act
            await repository.AddAsync(motorcycle);

            // Assert
            var addedMotorcycle = await context.Motorcycles.FirstOrDefaultAsync(m => m.LicensePlate == "TEST1234");
            Assert.NotNull(addedMotorcycle);
            Assert.Equal("TEST1234", addedMotorcycle.LicensePlate);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMotorcycle_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Year = 2023, Model = "Model Z", LicensePlate = "GETBYID" };
            await repository.AddAsync(motorcycle);

            // Act
            var foundMotorcycle = await repository.GetByIdAsync(motorcycle.Id);

            // Assert
            Assert.NotNull(foundMotorcycle);
            Assert.Equal(motorcycle.LicensePlate, foundMotorcycle.LicensePlate);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);

            // Act
            var foundMotorcycle = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(foundMotorcycle);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllMotorcycles_WhenLicensePlateIsNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            await repository.AddAsync(new Motorcycle { Year = 2020, Model = "M1", LicensePlate = "AAA1111" });
            await repository.AddAsync(new Motorcycle { Year = 2021, Model = "M2", LicensePlate = "BBB2222" });

            // Act
            var motorcycles = await repository.GetAllAsync(null);

            // Assert
            Assert.Equal(2, motorcycles.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnFilteredMotorcycles_WhenLicensePlateIsProvided()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            await repository.AddAsync(new Motorcycle { Year = 2020, Model = "M1", LicensePlate = "AAA1111" });
            await repository.AddAsync(new Motorcycle { Year = 2021, Model = "M2", LicensePlate = "AAB2222" });
            await repository.AddAsync(new Motorcycle { Year = 2022, Model = "M3", LicensePlate = "CCC3333" });

            // Act
            var motorcycles = await repository.GetAllAsync("AA");

            // Assert
            Assert.Equal(2, motorcycles.Count);
            Assert.Contains(motorcycles, m => m.LicensePlate == "AAA1111");
            Assert.Contains(motorcycles, m => m.LicensePlate == "AAB2222");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMotorcycle()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Year = 2023, Model = "Model Update", LicensePlate = "OLDPLATE" };
            await repository.AddAsync(motorcycle);

            motorcycle.LicensePlate = "NEWPLATE";

            // Act
            await repository.UpdateAsync(motorcycle);

            // Assert
            var updatedMotorcycle = await context.Motorcycles.FindAsync(motorcycle.Id);
            Assert.NotNull(updatedMotorcycle);
            Assert.Equal("NEWPLATE", updatedMotorcycle.LicensePlate);
        }

        [Fact]
        public async Task HasRentalsAsync_ShouldReturnTrue_WhenRentalsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "Model R", LicensePlate = "RENTAL" };
            await repository.AddAsync(motorcycle);

            var deliveryDriver = new DeliveryDriver
            {
                Id = 1,
                Name = "Test Driver",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                Role = UserRoleEnum.DeliveryDriver,
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "image_url"
            };
            context.DeliveryDrivers.Add(deliveryDriver);
            await context.SaveChangesAsync();

            var rental = new Rental
            {
                DeliveryDriverId = deliveryDriver.Id,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = motorcycle.Id,
                Motorcycle = motorcycle,
                StartDate = DateTime.UtcNow,
                ExpectedReturnDate = DateTime.UtcNow.AddDays(1),
                PlanDays = 1,
                DailyRate = 10.00m,
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            await context.Rentals.AddAsync(rental);
            await context.SaveChangesAsync();

            // Act
            var hasRentals = await repository.HasRentalsAsync(motorcycle.Id);

            // Assert
            Assert.True(hasRentals);
        }

        [Fact]
        public async Task HasRentalsAsync_ShouldReturnFalse_WhenNoRentalsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Year = 2023, Model = "Model NR", LicensePlate = "NORENTAL" };
            await repository.AddAsync(motorcycle);

            // Act
            var hasRentals = await repository.HasRentalsAsync(motorcycle.Id);

            // Assert
            Assert.False(hasRentals);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveMotorcycle()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new MotorcycleRepository(context);
            var motorcycle = new Motorcycle { Year = 2023, Model = "Model D", LicensePlate = "DELETE" };
            await repository.AddAsync(motorcycle);

            // Act
            await repository.DeleteAsync(motorcycle);

            // Assert
            var deletedMotorcycle = await context.Motorcycles.FindAsync(motorcycle.Id);
            Assert.Null(deletedMotorcycle);
        }
    }
}