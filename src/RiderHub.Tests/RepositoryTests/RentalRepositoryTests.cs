using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Repositories;
using Xunit;
using RiderHub.Domain.Enums;

namespace RiderHub.Tests.RepositoryTests
{
    public class RentalRepositoryTests
    {
        private RiderHubContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RiderHubContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RiderHubContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddRental()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);

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

            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            context.Motorcycles.Add(motorcycle);
            await context.SaveChangesAsync();

            var rental = new Rental
            {
                DeliveryDriverId = deliveryDriver.Id,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = motorcycle.Id,
                Motorcycle = motorcycle,
                StartDate = DateTime.UtcNow.Date,
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(7),
                PlanDays = 7,
                DailyRate = 30.00m,
                EndDate = DateTime.UtcNow.Date.AddDays(7)
            };

            // Act
            await repository.AddAsync(rental);

            // Assert
            var addedRental = await context.Rentals.FirstOrDefaultAsync(r => r.Id == rental.Id);
            Assert.NotNull(addedRental);
            Assert.Equal(rental.MotorcycleId, addedRental.MotorcycleId);
        }

        [Fact]
        public async Task IsMotorcycleAvailableAsync_ShouldReturnTrue_WhenNoConflictingRentals()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);
            var motorcycleId = 1;
            var startDate = DateTime.UtcNow.Date.AddDays(10);
            var endDate = DateTime.UtcNow.Date.AddDays(17);

            // Act
            var isAvailable = await repository.IsMotorcycleAvailableAsync(motorcycleId, startDate, endDate);

            // Assert
            Assert.True(isAvailable);
        }

        [Fact]
        public async Task IsMotorcycleAvailableAsync_ShouldReturnFalse_WhenConflictingRentalsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);

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

            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            context.Motorcycles.Add(motorcycle);
            await context.SaveChangesAsync();

            var existingRental = new Rental
            {
                DeliveryDriverId = deliveryDriver.Id,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = motorcycle.Id,
                Motorcycle = motorcycle,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(15),
                PlanDays = 10,
                DailyRate = 25.00m,
                EndDate = DateTime.UtcNow.Date.AddDays(15)
            };
            await repository.AddAsync(existingRental);

            var startDate = DateTime.UtcNow.Date.AddDays(12);
            var endDate = DateTime.UtcNow.Date.AddDays(20);

            // Act
            var isAvailable = await repository.IsMotorcycleAvailableAsync(motorcycle.Id, startDate, endDate);

            // Assert
            Assert.False(isAvailable);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRental_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);

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

            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            context.Motorcycles.Add(motorcycle);
            await context.SaveChangesAsync();

            var rental = new Rental
            {
                DeliveryDriverId = deliveryDriver.Id,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = motorcycle.Id,
                Motorcycle = motorcycle,
                StartDate = DateTime.UtcNow.Date,
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(7),
                PlanDays = 7,
                DailyRate = 30.00m,
                EndDate = DateTime.UtcNow.Date.AddDays(7)
            };
            await repository.AddAsync(rental);

            // Act
            var foundRental = await repository.GetByIdAsync(rental.Id);

            // Assert
            Assert.NotNull(foundRental);
            Assert.Equal(rental.MotorcycleId, foundRental.MotorcycleId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);

            // Act
            var foundRental = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(foundRental);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRental()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalRepository(context);

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

            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            context.Motorcycles.Add(motorcycle);
            await context.SaveChangesAsync();

            var rental = new Rental
            {
                DeliveryDriverId = deliveryDriver.Id,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = motorcycle.Id,
                Motorcycle = motorcycle,
                StartDate = DateTime.UtcNow.Date,
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(7),
                PlanDays = 7,
                DailyRate = 30.00m,
                EndDate = DateTime.UtcNow.Date.AddDays(7)
            };
            await repository.AddAsync(rental);

            rental.EndDate = DateTime.UtcNow.Date.AddDays(5);

            // Act
            await repository.UpdateAsync(rental);

            // Assert
            var updatedRental = await context.Rentals.FindAsync(rental.Id);
            Assert.NotNull(updatedRental);
            Assert.Equal(rental.EndDate, updatedRental.EndDate);
        }
    }
}