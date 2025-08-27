using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Infrastructure.Context;
using RiderHub.Infrastructure.Repositories;
using Xunit;

namespace RiderHub.Tests.RepositoryTests
{
    public class RentalPlanRepositoryTests
    {
        private RiderHubContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RiderHubContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RiderHubContext(options);
        }

        [Fact]
        public async Task GetByDaysAsync_ShouldReturnRentalPlan_WhenFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalPlanRepository(context);
            var rentalPlan = new RentalPlan { Days = 7, DailyRate = 30.00m };
            await context.RentalPlans.AddAsync(rentalPlan);
            await context.SaveChangesAsync();

            // Act
            var foundPlan = await repository.GetByDaysAsync(7);

            // Assert
            Assert.NotNull(foundPlan);
            Assert.Equal(7, foundPlan.Days);
        }

        [Fact]
        public async Task GetByDaysAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new RentalPlanRepository(context);

            // Act
            var foundPlan = await repository.GetByDaysAsync(99);

            // Assert
            Assert.Null(foundPlan);
        }
    }
}
