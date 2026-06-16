using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure;
using GLMS.Infrastructure.Repository;
using GLMS.Infrastructure.Storage;
using GLMS.Web.Controllers.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GLMS.Tests.Controllers
{
    public class ContractsApiControllerTests
    {
        private readonly GLMSDbContext _context;
        private readonly ContractsApiController _controller;

        public ContractsApiControllerTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseSqlServer(
                    "server=Justice;database=GLMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;")
                .Options;

            _context = new GLMSDbContext(options);

            _context.Database.EnsureCreated();

            IContractRepository repository =
                new ContractRepository(_context);

            var environment = new Mock<IWebHostEnvironment>();

            environment.Setup(x => x.ContentRootPath)
                .Returns(AppContext.BaseDirectory);

            _controller = new ContractsApiController(
                repository,
                environment.Object,
                NullLogger<ContractsApiController>.Instance);
        }

        private async Task<Client> CreateClientAsync()
        {
            var client = new Client
            {
                Name = $"Client-{Guid.NewGuid()}",
                ContactDetails = "Test Contact",
                Region = "Test Region"
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        [Fact]
        public async Task GetAll_Returns_Contracts()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(
                null,
                null,
                null,
                null,
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var contracts =
                Assert.IsAssignableFrom<IEnumerable<Contract>>(
                    okResult.Value);

            Assert.Contains(
                contracts,
                c => c.Id == contract.Id);
        }

        [Fact]
        public async Task GetById_Returns_Contract_When_Found()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetById(
                contract.Id,
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var returnedContract =
                Assert.IsType<Contract>(okResult.Value);

            Assert.Equal(
                contract.Id,
                returnedContract.Id);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Missing()
        {
            // Act
            var result = await _controller.GetById(
                int.MaxValue,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Adds_Contract()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            // Act
            var result = await _controller.Create(
                contract,
                CancellationToken.None);

            // Assert
            var created =
                Assert.IsType<CreatedAtActionResult>(
                    result.Result);

            var createdContract =
                Assert.IsType<Contract>(created.Value);

            var exists = await _context.Contracts
                .AnyAsync(c => c.Id == createdContract.Id);

            Assert.True(exists);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_EndDate_Is_Before_StartDate()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(-1),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            // Act
            var result = await _controller.Create(
                contract,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Update_Returns_NoContent()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            contract.ServiceLevel = "Platinum";

            // Act
            var result = await _controller.Update(
                contract.Id,
                contract,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updated =
                await _context.Contracts.FindAsync(contract.Id);

            Assert.Equal(
                "Platinum",
                updated!.ServiceLevel);
        }

        [Fact]
        public async Task Update_Returns_BadRequest_When_Ids_Do_Not_Match()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                Id = 1,
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            // Act
            var result = await _controller.Update(
                999,
                contract,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result);
        }

        [Fact]
        public async Task Delete_Removes_Contract()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12),
                Status = ContractStatus.Active,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(
                contract.Id,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var deleted =
                await _context.Contracts.FindAsync(
                    contract.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Missing()
        {
            // Act
            var result = await _controller.Delete(
                int.MaxValue,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(
                result);
        }

        [Fact]
        public async Task GetStatistics_Returns_Ok()
        {
            // Act
            var result = await _controller.GetStatistics(
                CancellationToken.None);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClients_Returns_Ok()
        {
            // Act
            var result = await _controller.GetClients(
                CancellationToken.None);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}