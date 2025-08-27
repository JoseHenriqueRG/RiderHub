using Moq;
using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using RiderHub.Application.Services;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using Xunit;

namespace RiderHub.Tests.UnitTests
{
    public class MotorcycleServiceUnitTests
    {
        private readonly Mock<IMotorcycleRepository> _mockMotorcycleRepository;
        private readonly Mock<IMessagePublisher> _mockMessagePublisher;
        private readonly MotorcycleService _motorcycleService;

        public MotorcycleServiceUnitTests()
        {
            _mockMotorcycleRepository = new Mock<IMotorcycleRepository>();
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _motorcycleService = new MotorcycleService(_mockMotorcycleRepository.Object, _mockMessagePublisher.Object);
        }

        [Fact]
        public async Task CreateMotorcycleAsync_ShouldReturnMotorcycle_WhenSuccessful()
        {
            // Arrange
            var dto = new CreateMotorcycleDto { Year = 2023, Model = "Model X", LicensePlate = "ABC1234" };
            _mockMotorcycleRepository.Setup(repo => repo.AddAsync(It.IsAny<Motorcycle>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _motorcycleService.CreateMotorcycleAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.LicensePlate, result.LicensePlate);
            _mockMotorcycleRepository.Verify(repo => repo.AddAsync(It.IsAny<Motorcycle>()), Times.Once);
            _mockMessagePublisher.Verify(pub => pub.Publish(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateMotorcycleAsync_ShouldThrowInvalidOperationException_WhenLicensePlateExists()
        {
            // Arrange
            var dto = new CreateMotorcycleDto { Year = 2023, Model = "Model X", LicensePlate = "ABC1234" };
            _mockMotorcycleRepository.Setup(repo => repo.AddAsync(It.IsAny<Motorcycle>()))
                .ThrowsAsync(new InvalidOperationException("A motorcycle with this license plate already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _motorcycleService.CreateMotorcycleAsync(dto));
            _mockMotorcycleRepository.Verify(repo => repo.AddAsync(It.IsAny<Motorcycle>()), Times.Once);
            _mockMessagePublisher.Verify(pub => pub.Publish(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetMotorcyclesAsync_ShouldReturnAllMotorcycles_WhenPlateIsNull()
        {
            // Arrange
            var motorcycles = new List<Motorcycle> { new Motorcycle { Id = 1, Year = 2023, Model = "Model X", LicensePlate = "ABC1234" } };
            _mockMotorcycleRepository.Setup(repo => repo.GetAllAsync(null)).ReturnsAsync(motorcycles);

            // Act
            var result = await _motorcycleService.GetMotorcyclesAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockMotorcycleRepository.Verify(repo => repo.GetAllAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetMotorcyclesAsync_ShouldReturnFilteredMotorcycles_WhenPlateIsProvided()
        {
            // Arrange
            var motorcycles = new List<Motorcycle> { new Motorcycle { Id = 1, Year = 2023, Model = "Model X", LicensePlate = "ABC1234" } };
            _mockMotorcycleRepository.Setup(repo => repo.GetAllAsync("ABC")).ReturnsAsync(motorcycles);

            // Act
            var result = await _motorcycleService.GetMotorcyclesAsync("ABC");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockMotorcycleRepository.Verify(repo => repo.GetAllAsync("ABC"), Times.Once);
        }

        [Fact]
        public async Task UpdateMotorcycleLicensePlateAsync_ShouldUpdateLicensePlate_WhenMotorcycleExists()
        {
            // Arrange
            var dto = new UpdateMotorcycleLicensePlateDto { Id = 1, LicensePlate = "DEF5678" };
            var motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "Model X", LicensePlate = "ABC1234" };
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockMotorcycleRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Motorcycle>())).Returns(Task.CompletedTask);

            // Act
            await _motorcycleService.UpdateMotorcycleLicensePlateAsync(dto);

            // Assert
            Assert.Equal(dto.LicensePlate, motorcycle.LicensePlate);
            _mockMotorcycleRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.UpdateAsync(motorcycle), Times.Once);
        }

        [Fact]
        public async Task UpdateMotorcycleLicensePlateAsync_ShouldThrowInvalidOperationException_WhenMotorcycleNotFound()
        {
            // Arrange
            var dto = new UpdateMotorcycleLicensePlateDto { Id = 1, LicensePlate = "DEF5678" };
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Motorcycle)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _motorcycleService.UpdateMotorcycleLicensePlateAsync(dto));
            _mockMotorcycleRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Motorcycle>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMotorcycleAsync_ShouldDeleteMotorcycle_WhenNoRentalsExist()
        {
            // Arrange
            var motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "Model X", LicensePlate = "ABC1234" };
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockMotorcycleRepository.Setup(repo => repo.HasRentalsAsync(1)).ReturnsAsync(false);
            _mockMotorcycleRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Motorcycle>())).Returns(Task.CompletedTask);

            // Act
            await _motorcycleService.DeleteMotorcycleAsync(1);

            // Assert
            _mockMotorcycleRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.HasRentalsAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.DeleteAsync(motorcycle), Times.Once);
        }

        [Fact]
        public async Task DeleteMotorcycleAsync_ShouldThrowInvalidOperationException_WhenMotorcycleNotFound()
        {
            // Arrange
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Motorcycle)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _motorcycleService.DeleteMotorcycleAsync(1));
            _mockMotorcycleRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.HasRentalsAsync(It.IsAny<int>()), Times.Never);
            _mockMotorcycleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Motorcycle>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMotorcycleAsync_ShouldThrowInvalidOperationException_WhenRentalsExist()
        {
            // Arrange
            var motorcycle = new Motorcycle { Id = 1, Year = 2023, Model = "Model X", LicensePlate = "ABC1234" };
            _mockMotorcycleRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(motorcycle);
            _mockMotorcycleRepository.Setup(repo => repo.HasRentalsAsync(1)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _motorcycleService.DeleteMotorcycleAsync(1));
            _mockMotorcycleRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.HasRentalsAsync(1), Times.Once);
            _mockMotorcycleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Motorcycle>()), Times.Never);
        }
    }
}