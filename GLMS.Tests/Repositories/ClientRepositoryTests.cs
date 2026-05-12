using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Infrastructure;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.Repositories
{
    public class ClientRepositoryTests : IDisposable
    {
        private readonly GLMSDbContext _context;
        private readonly ClientRepository _repository;

        public ClientRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GLMSDbContext(options);

            _repository = new ClientRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var clients = new List<Client>
            {
                new Client
                {
                    Id = 1,
                    Name = "Alpha Corp",
                    ContactDetails = "alpha@test.com",
                    Region = "Pretoria"
                },

                new Client
                {
                    Id = 2,
                    Name = "Beta Holdings",
                    ContactDetails = "beta@test.com",
                    Region = "Johannesburg"
                }
            };

            _context.Clients.AddRange(clients);

            _context.SaveChanges();
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllClients()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();

            result.Should().HaveCount(2);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnClient_WhenExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.Name.Should().Be("Alpha Corp");
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

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenMissing()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ShouldAddClient()
        {
            // Arrange
            var client = new Client
            {
                Name = "Gamma Ltd",
                ContactDetails = "gamma@test.com",
                Region = "Cape Town"
            };

            // Act
            var result = await _repository.CreateAsync(client);

            // Assert
            result.Should().NotBeNull();

            _context.Clients.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenClientNull()
        {
            // Act
            Func<Task> action = async () =>
                await _repository.CreateAsync(null!);

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentNullException>();
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldUpdateClient()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Name = "Updated Client",
                ContactDetails = "updated@test.com",
                Region = "Durban"
            };

            // Act
            var result = await _repository.UpdateAsync(client);

            // Assert
            result.Name.Should().Be("Updated Client");

            result.Region.Should().Be("Durban");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenClientMissing()
        {
            // Arrange
            var client = new Client
            {
                Id = 999,
                Name = "Missing"
            };

            // Act
            Func<Task> action = async () =>
                await _repository.UpdateAsync(client);

            // Assert
            await action.Should()
                .ThrowAsync<KeyNotFoundException>();
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ShouldRemoveClient_WhenExists()
        {
            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();

            _context.Clients.Should().HaveCount(1);
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

        #region GetDetailsAsync

        [Fact]
        public async Task GetDetailsAsync_ShouldReturnClientWithContracts()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();

            result!.Contracts.Should().NotBeNull();

            result.Contracts.Should().HaveCount(1);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();

            _context.Dispose();
        }
    }
}