using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Infrastructure.Repository;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.Repositories
{
    public class ServiceRequestRepositoryTests : IDisposable
    {
        private readonly GLMSDbContext _context;
        private readonly ServiceRequestRepository _repository;

        public ServiceRequestRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new GLMSDbContext(options);

            _repository = new ServiceRequestRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var client = new Client
            {
                Id = 1,
                Name = "Test Client"
            };

            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                Client = client,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            var request = new ServiceRequest
            {
                Id = 1,
                ContractId = 1,
                Contract = contract,
                Title = "Server Issue",
                Description = "Investigate outage",
                Status = "Open",
                AmountUSD = 100,
                LocalCostZAR = 1800
            };

            _context.Clients.Add(client);

            _context.Contracts.Add(contract);

            _context.ServiceRequests.Add(request);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnRequests()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRequest_WhenExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.Title.Should().Be("Server Issue");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDetailsAsync_ShouldIncludeContractAndClient()
        {
            // Act
            var result = await _repository.GetDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.Contract.Should().NotBeNull();

            result.Contract!.Client.Should().NotBeNull();

            result.Contract.Client!.Name.Should().Be("Test Client");
        }

        [Fact]
        public async Task CreateAsync_ShouldAddRequest()
        {
            // Arrange
            var request = new ServiceRequest
            {
                ContractId = 1,
                Title = "New Request",
                Description = "Description",
                Status = "Open",
                AmountUSD = 50,
                LocalCostZAR = 900
            };

            // Act
            await _repository.CreateAsync(request);

            // Assert
            _context.ServiceRequests.Count()
                .Should().Be(2);
        }

        [Fact]
        public async Task GetContractsAsync_ShouldReturnContracts()
        {
            // Act
            var result = await _repository.GetContractsAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetContractAsync_ShouldReturnContract()
        {
            // Act
            var result = await _repository.GetContractAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.ServiceLevel.Should().Be("Gold");
        }

        [Fact]
        public async Task GetContractAsync_ShouldReturnNull_WhenMissing()
        {
            // Act
            var result = await _repository.GetContractAsync(999);

            // Assert
            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();

            _context.Dispose();
        }
    }
}