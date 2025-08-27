using Moq;
using RiderHub.Application.Dtos;
using RiderHub.Application.Services;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Enums;
using RiderHub.Domain.Interfaces;

namespace RiderHub.Tests.UnitTests
{
    public class RentalServiceUnitTests
    {
        private readonly Mock<IRentalRepository> _mockRentalRepository;
        private readonly Mock<IMotorcycleRepository> _mockMotorcycleRepository;
        private readonly Mock<IDeliveryDriverRepository> _mockDeliveryDriverRepository;
        private readonly Mock<IRentalPlanRepository> _mockRentalPlanRepository;
        private readonly RentalService _rentalService;

        public RentalServiceUnitTests()
        {
            _mockRentalRepository = new Mock<IRentalRepository>();
            _mockMotorcycleRepository = new Mock<IMotorcycleRepository>();
            _mockDeliveryDriverRepository = new Mock<IDeliveryDriverRepository>();
            _mockRentalPlanRepository = new Mock<IRentalPlanRepository>();
            _rentalService = new RentalService(
                _mockRentalRepository.Object,
                _mockMotorcycleRepository.Object,
                _mockDeliveryDriverRepository.Object,
                _mockRentalPlanRepository.Object
            );
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldCreateRentalSuccessfully()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 7 };
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
            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            var rentalPlan = new RentalPlan { Days = 7, DailyRate = 30.00m };

            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockRentalPlanRepository.Setup(repo => repo.GetByDaysAsync(7)).ReturnsAsync(rentalPlan);
            _mockRentalRepository.Setup(repo => repo.IsMotorcycleAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(true);
            _mockRentalRepository.Setup(repo => repo.AddAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CreateRentalAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.DeliveryDriverId, result.DeliveryDriverId);
            Assert.Equal(dto.MotorcycleId, result.MotorcycleId);
            Assert.Equal(rentalPlan.DailyRate, result.DailyRate);
            _mockRentalRepository.Verify(repo => repo.AddAsync(It.IsAny<Rental>()), Times.Once);
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldThrowException_WhenDeliveryDriverNotFound()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 7 };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((DeliveryDriver)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(dto));
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldThrowException_WhenMotorcycleNotFound()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 7 };
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
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Motorcycle)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(dto));
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldThrowException_WhenInvalidCnhType()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 7 };
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
                DriverLicenseType = DriverLicenseTypeEnum.B, // Invalid CNH type
                DriverLicenseImage = "image_url"
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(dto));
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldThrowException_WhenInvalidRentalPlanDays()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 99 }; // Invalid plan
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
            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };

            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockRentalPlanRepository.Setup(repo => repo.GetByDaysAsync(99)).ReturnsAsync((RentalPlan)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(dto));
        }

        [Fact]
        public async Task CreateRentalAsync_ShouldThrowException_WhenMotorcycleNotAvailable()
        {
            // Arrange
            var dto = new CreateRentalDto { DeliveryDriverId = 1, MotorcycleId = 1, PlanDays = 7 };
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
            var motorcycle = new Motorcycle
            {
                Id = 1,
                Year = 2023,
                Model = "TestModel",
                LicensePlate = "TEST123"
            };
            var rentalPlan = new RentalPlan { Days = 7, DailyRate = 30.00m };

            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockRentalPlanRepository.Setup(repo => repo.GetByDaysAsync(7)).ReturnsAsync(rentalPlan);
            _mockRentalRepository.Setup(repo => repo.IsMotorcycleAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(dto));
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldCalculateCorrectly_OnTimeReturn()
        {
            // Arrange
            var rental = new Rental
            {
                Id = 1,
                DeliveryDriverId = 1,
                MotorcycleId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(8), // 7 days plan
                PlanDays = 7,
                DailyRate = 30.00m,
                DeliveryDriver = new DeliveryDriver { Id = 1, Name = "Test Driver", Email = "test@example.com", PasswordHash = "hashed_password", Role = UserRoleEnum.DeliveryDriver, CNPJ = "12345678901234", DateOfBirth = new DateTime(1990, 1, 1), DriverLicenseNumber = "123456789", DriverLicenseType = DriverLicenseTypeEnum.A, DriverLicenseImage = "image_url" },
                Motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "TestModel", LicensePlate = "TEST123" }
            };
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date.AddDays(8) };

            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(rental);
            _mockRentalRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CalculateRentalCostAsync(dto);

            // Assert
            Assert.Equal(7 * 30.00m, result);
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldCalculateCorrectly_EarlyReturn_7DaysPlan()
        {
            // Arrange
            var rental = new Rental
            {
                Id = 1,
                DeliveryDriverId = 1,
                MotorcycleId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(8), // 7 days plan
                PlanDays = 7,
                DailyRate = 30.00m,
                DeliveryDriver = new DeliveryDriver { Id = 1, Name = "Test Driver", Email = "test@example.com", PasswordHash = "hashed_password", Role = UserRoleEnum.DeliveryDriver, CNPJ = "12345678901234", DateOfBirth = new DateTime(1990, 1, 1), DriverLicenseNumber = "123456789", DriverLicenseType = DriverLicenseTypeEnum.A, DriverLicenseImage = "image_url" },
                Motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "TestModel", LicensePlate = "TEST123" }
            };
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date.AddDays(5) }; // Return 3 days early

            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(rental);
            _mockRentalRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CalculateRentalCostAsync(dto);

            // Assert
            var actualDaysRented = 4; // Day 1 to Day 4
            var remainingDays = 3; // Day 5 to Day 7
            var expectedCost = (actualDaysRented * 30.00m) + (remainingDays * 30.00m * 0.20m);
            Assert.Equal(expectedCost, result);
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldCalculateCorrectly_EarlyReturn_15DaysPlan()
        {
            // Arrange
            var rental = new Rental
            {
                Id = 1,
                DeliveryDriverId = 1,
                MotorcycleId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(16), // 15 days plan
                PlanDays = 15,
                DailyRate = 28.00m,
                DeliveryDriver = new DeliveryDriver { Id = 1, Name = "Test Driver", Email = "test@example.com", PasswordHash = "hashed_password", Role = UserRoleEnum.DeliveryDriver, CNPJ = "12345678901234", DateOfBirth = new DateTime(1990, 1, 1), DriverLicenseNumber = "123456789", DriverLicenseType = DriverLicenseTypeEnum.A, DriverLicenseImage = "image_url" },
                Motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "TestModel", LicensePlate = "TEST123" }
            };
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date.AddDays(10) }; // Return 6 days early

            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(rental);
            _mockRentalRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CalculateRentalCostAsync(dto);

            // Assert
            var actualDaysRented = 9; // Day 1 to Day 9
            var remainingDays = 6; // Day 10 to Day 15
            var expectedCost = (actualDaysRented * 28.00m) + (remainingDays * 28.00m * 0.40m);
            Assert.Equal(expectedCost, result);
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldCalculateCorrectly_LateReturn()
        {
            // Arrange
            var rental = new Rental
            {
                Id = 1,
                DeliveryDriverId = 1,
                MotorcycleId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(8), // 7 days plan
                PlanDays = 7,
                DailyRate = 30.00m,
                DeliveryDriver = new DeliveryDriver { Id = 1, Name = "Test Driver", Email = "test@example.com", PasswordHash = "hashed_password", Role = UserRoleEnum.DeliveryDriver, CNPJ = "12345678901234", DateOfBirth = new DateTime(1990, 1, 1), DriverLicenseNumber = "123456789", DriverLicenseType = DriverLicenseTypeEnum.A, DriverLicenseImage = "image_url" },
                Motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "TestModel", LicensePlate = "TEST123" }
            };
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date.AddDays(10) }; // Return 2 days late

            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(rental);
            _mockRentalRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

            // Act
            var result = await _rentalService.CalculateRentalCostAsync(dto);

            // Assert
            var expectedCost = (7 * 30.00m) + (2 * 50.00m);
            Assert.Equal(expectedCost, result);
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldThrowException_WhenRentalNotFound()
        {
            // Arrange
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date };
            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Rental)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CalculateRentalCostAsync(dto));
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task CalculateRentalCostAsync_ShouldThrowException_WhenReturnDateBeforeStartDate()
        {
            // Arrange
            var rental = new Rental
            {
                Id = 1,
                DeliveryDriverId = 1,
                MotorcycleId = 1,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(12), // 7 days plan
                PlanDays = 7,
                DailyRate = 30.00m,
                DeliveryDriver = new DeliveryDriver { Id = 1, Name = "Test Driver", Email = "test@example.com", PasswordHash = "hashed_password", Role = UserRoleEnum.DeliveryDriver, CNPJ = "12345678901234", DateOfBirth = new DateTime(1990, 1, 1), DriverLicenseNumber = "123456789", DriverLicenseType = DriverLicenseTypeEnum.A, DriverLicenseImage = "image_url" },
                Motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "TestModel", LicensePlate = "TEST123" }
            };
            var dto = new ReturnRentalDto { RentalId = 1, ReturnDate = DateTime.UtcNow.Date.AddDays(4) }; // Before start date

            _mockRentalRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(rental);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CalculateRentalCostAsync(dto));
            _mockRentalRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rental>()), Times.Never);
        }
    }
}