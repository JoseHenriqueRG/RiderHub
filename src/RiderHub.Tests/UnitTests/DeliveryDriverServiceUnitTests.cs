using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using RiderHub.Application.Dtos;
using RiderHub.Application.Services;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Enums;
using RiderHub.Domain.Exceptions;
using RiderHub.Domain.Interfaces;
using System.Text;

namespace RiderHub.Tests.UnitTests
{
    public class DeliveryDriverServiceUnitTests
    {
        private readonly Mock<IDeliveryDriverRepository> _mockDeliveryDriverRepository;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly DeliveryDriverService _deliveryDriverService;

        public DeliveryDriverServiceUnitTests()
        {
            _mockDeliveryDriverRepository = new Mock<IDeliveryDriverRepository>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockWebHostEnvironment.Setup(env => env.WebRootPath).Returns("C:\\temp"); 
            _deliveryDriverService = new DeliveryDriverService(_mockDeliveryDriverRepository.Object, _mockWebHostEnvironment.Object);
        }

        private IFormFile CreateMockFormFile(string fileName, string content, string contentType)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(stream.Length);
            formFile.Setup(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns((Stream s, CancellationToken t) => stream.CopyToAsync(s));
            return formFile.Object;
        }

        [Fact]
        public async Task RegisterDeliveryDriverAsync_ShouldRegisterSuccessfully()
        {
            // Arrange
            var dto = new RegisterDeliveryDriverDto
            {
                Name = "Test Driver",
                Email = "test@example.com",
                Password = "Password123",
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = CreateMockFormFile("cnh.png", "image_data", "image/png")
            };

            _mockDeliveryDriverRepository.Setup(repo => repo.GetByCNPJAsync(It.IsAny<string>())).ReturnsAsync((DeliveryDriver)null);
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByDriverLicenseNumberAsync(It.IsAny<string>())).ReturnsAsync((DeliveryDriver)null);
            _mockDeliveryDriverRepository.Setup(repo => repo.AddAsync(It.IsAny<DeliveryDriver>())).Returns(Task.CompletedTask);

            // Act
            var result = await _deliveryDriverService.RegisterDeliveryDriverAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            _mockDeliveryDriverRepository.Verify(repo => repo.AddAsync(It.IsAny<DeliveryDriver>()), Times.Once);
        }

        [Fact]
        public async Task RegisterDeliveryDriverAsync_ShouldThrowException_WhenInvalidCnpj()
        {
            // Arrange
            var dto = new RegisterDeliveryDriverDto
            {
                Name = "Test Driver",
                Email = "test@example.com",
                Password = "Password123",
                CNPJ = "invalid",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = CreateMockFormFile("cnh.png", "image_data", "image/png")
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _deliveryDriverService.RegisterDeliveryDriverAsync(dto));
        }

        [Fact]
        public async Task RegisterDeliveryDriverAsync_ShouldThrowException_WhenCnpjAlreadyExists()
        {
            // Arrange
            var dto = new RegisterDeliveryDriverDto
            {
                Name = "Test Driver",
                Email = "test@example.com",
                Password = "Password123",
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = CreateMockFormFile("cnh.png", "image_data", "image/png")
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByCNPJAsync(It.IsAny<string>())).ReturnsAsync(new DeliveryDriver
            {
                Id = 1,
                Name = "Existing Driver",
                Email = "existing@example.com",
                PasswordHash = "hashed_password",
                Role = UserRoleEnum.DeliveryDriver,
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1980, 1, 1),
                DriverLicenseNumber = "987654321",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "existing_image_url"
            });

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateCnpjException>(() => _deliveryDriverService.RegisterDeliveryDriverAsync(dto));
        }

        [Fact]
        public async Task RegisterDeliveryDriverAsync_ShouldThrowException_WhenCnhAlreadyExists()
        {
            // Arrange
            var dto = new RegisterDeliveryDriverDto
            {
                Name = "Test Driver",
                Email = "test@example.com",
                Password = "Password123",
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = CreateMockFormFile("cnh.png", "image_data", "image/png")
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByCNPJAsync(It.IsAny<string>())).ReturnsAsync((DeliveryDriver)null);
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByDriverLicenseNumberAsync(It.IsAny<string>())).ReturnsAsync(new DeliveryDriver
            {
                Id = 1,
                Name = "Existing Driver",
                Email = "existing@example.com",
                PasswordHash = "hashed_password",
                Role = UserRoleEnum.DeliveryDriver,
                CNPJ = "99999999999999",
                DateOfBirth = new DateTime(1980, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = "existing_image_url"
            });

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateCnhException>(() => _deliveryDriverService.RegisterDeliveryDriverAsync(dto));
        }

        [Fact]
        public async Task RegisterDeliveryDriverAsync_ShouldThrowException_WhenInvalidCnhImageFormat()
        {
            // Arrange
            var dto = new RegisterDeliveryDriverDto
            {
                Name = "Test Driver",
                Email = "test@example.com",
                Password = "Password123",
                CNPJ = "12345678901234",
                DateOfBirth = new DateTime(1990, 1, 1),
                DriverLicenseNumber = "123456789",
                DriverLicenseType = DriverLicenseTypeEnum.A,
                DriverLicenseImage = CreateMockFormFile("cnh.txt", "text_data", "text/plain") // Invalid format
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByCNPJAsync(It.IsAny<string>())).ReturnsAsync((DeliveryDriver)null);
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByDriverLicenseNumberAsync(It.IsAny<string>())).ReturnsAsync((DeliveryDriver)null);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _deliveryDriverService.RegisterDeliveryDriverAsync(dto));
        }

        [Fact]
        public async Task UpdateCnhImageAsync_ShouldUpdateSuccessfully()
        {
            // Arrange
            var dto = new UpdateCnhImageDto
            {
                DeliveryDriverId = 1,
                CnhImageFile = CreateMockFormFile("new_cnh.bmp", "image_data", "image/bmp")
            };
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
                DriverLicenseImage = "old_image.png"
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);
            _mockDeliveryDriverRepository.Setup(repo => repo.UpdateAsync(It.IsAny<DeliveryDriver>())).Returns(Task.CompletedTask);

            // Act
            await _deliveryDriverService.UpdateCnhImageAsync(dto);

            // Assert
            Assert.Contains("/cnh_images/", deliveryDriver.DriverLicenseImage);
            _mockDeliveryDriverRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockDeliveryDriverRepository.Verify(repo => repo.UpdateAsync(deliveryDriver), Times.Once);
        }

        [Fact]
        public async Task UpdateCnhImageAsync_ShouldThrowException_WhenDriverNotFound()
        {
            // Arrange
            var dto = new UpdateCnhImageDto
            {
                DeliveryDriverId = 1,
                CnhImageFile = CreateMockFormFile("new_cnh.bmp", "image_data", "image/bmp")
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((DeliveryDriver)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _deliveryDriverService.UpdateCnhImageAsync(dto));
            _mockDeliveryDriverRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockDeliveryDriverRepository.Verify(repo => repo.UpdateAsync(It.IsAny<DeliveryDriver>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCnhImageAsync_ShouldThrowException_WhenInvalidImageFormat()
        {
            // Arrange
            var dto = new UpdateCnhImageDto
            {
                DeliveryDriverId = 1,
                CnhImageFile = CreateMockFormFile("new_cnh.gif", "image_data", "image/gif") // Invalid format
            };
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
                DriverLicenseImage = "old_image.png"
            };
            _mockDeliveryDriverRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deliveryDriver);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _deliveryDriverService.UpdateCnhImageAsync(dto));
            _mockDeliveryDriverRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockDeliveryDriverRepository.Verify(repo => repo.UpdateAsync(It.IsAny<DeliveryDriver>()), Times.Never);
        }
    }
}