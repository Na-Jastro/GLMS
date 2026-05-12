using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Infrastructure.Repository;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.Repositories
{
    public class ContractRepositoryTests : IDisposable
    {
        private readonly GLMSDbContext _context;
        private readonly ContractRepository _repository;

        public ContractRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new GLMSDbContext(options);

            _repository = new ContractRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var client = new Client
            {
                Id = 1,
                Name = "Test Client",
                Region = "Pretoria"
            };

            var activeContract = new Contract
            {
                Id = 1,
                ClientId = 1,
                Client = client,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            var expiredContract = new Contract
            {
                Id = 2,
                ClientId = 1,
                Client = client,
                StartDate = DateTime.Today.AddDays(-60),
                EndDate = DateTime.Today.AddDays(-1),
                Status = ContractStatus.Expired,
                ServiceLevel = "Silver"
            };

            _context.Clients.Add(client);

            _context.Contracts.AddRange(
                activeContract,
                expiredContract);

            _context.SaveChanges();
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllContracts()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByStatus()
        {
            // Act
            var result = await _repository.GetAllAsync(
                status: ContractStatus.Active);

            // Assert
            result.Should().HaveCount(1);

            result.First().Status
                .Should().Be(ContractStatus.Active);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByClient()
        {
            // Act
            var result = await _repository.GetAllAsync(
                clientId: 1);

            // Assert
            result.Should().HaveCount(2);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnContract_WhenExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.ServiceLevel.Should().Be("Gold");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetDetailsAsync

        [Fact]
        public async Task GetDetailsAsync_ShouldIncludeClient()
        {
            // Act
            var result = await _repository.GetDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.Client.Should().NotBeNull();

            result.Client!.Name.Should().Be("Test Client");
        }

        [Fact]
        public async Task GetDetailsAsync_ShouldIncludeServiceRequests()
        {
            // Arrange
            _context.ServiceRequests.Add(
                new ServiceRequest
                {
                    Id = 1,
                    ContractId = 1,
                    Title = "Support Request",
                    Description = "Description",
                    Status = "Open",
                    AmountUSD = 100,
                    LocalCostZAR = 1800
                });

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDetailsAsync(1);

            // Assert
            result!.ServiceRequests.Should().NotBeNull();

            result.ServiceRequests.Should().HaveCount(1);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ShouldAddContract()
        {
            // Arrange
            var contract = new Contract
            {
                ClientId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(15),
                Status = ContractStatus.Active,
                ServiceLevel = "Bronze"
            };

            // Act
            await _repository.CreateAsync(contract);

            // Assert
            _context.Contracts.Count()
                .Should().Be(3);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldUpdateContract()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Updated SLA"
            };

            // Act
            var result = await _repository.UpdateAsync(contract);

            // Assert
            result.ServiceLevel
                .Should().Be("Updated SLA");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenContractMissing()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 999
            };

            // Act
            Func<Task> action = async () =>
                await _repository.UpdateAsync(contract);

            // Assert
            await action.Should()
                .ThrowAsync<KeyNotFoundException>();
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ShouldRemoveContract_WhenExists()
        {
            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();

            _context.Contracts.Count()
                .Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenMissing()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Counts

        [Fact]
        public async Task GetTotalCountAsync_ShouldReturnCount()
        {
            // Act
            var result = await _repository.GetTotalCountAsync();

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task GetActiveCountAsync_ShouldReturnActiveCount()
        {
            // Act
            var result = await _repository.GetActiveCountAsync();

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task GetExpiredCountAsync_ShouldReturnExpiredCount()
        {
            // Act
            var result = await _repository.GetExpiredCountAsync();

            // Assert
            result.Should().Be(1);
        }

        #endregion

        #region GetClientsAsync

        [Fact]
        public async Task GetClientsAsync_ShouldReturnClients()
        {
            // Act
            var result = await _repository.GetClientsAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();

            _context.Dispose();
        }
    }
}