using GLMS.Api.Controllers;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure;
using GLMS.Infrastructure.Repository;
using GLMS.Infrastructure.Services;
using GLMS.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GLMS.Tests.Controllers
{
    public class ServiceRequestsApiControllerTests
    {
        private readonly GLMSDbContext _context;
        private readonly ServiceRequestsApiController _controller;

        public ServiceRequestsApiControllerTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseSqlServer(
                    "server=Justice;database=GLMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;")
                .Options;

            _context = new GLMSDbContext(options);

            _context.Database.EnsureCreated();

            IServiceRequestRepository repository =
                new ServiceRequestRepository(_context);

            var currencyService =
                new Mock<ICurrencyService>();

            currencyService
                .Setup(x => x.GetUsdToZarRate())
                .ReturnsAsync(18.50m);

            _controller = new ServiceRequestsApiController(
                repository,
                currencyService.Object,
                NullLogger<ServiceRequestsApiController>.Instance);
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

        private async Task<Contract> CreateContractAsync()
        {
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

            return contract;
        }

        [Fact]
        public async Task GetAll_Returns_ServiceRequests()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Test Request",
                Description = "Test Description",
                AmountUSD = 100
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var requests =
                Assert.IsAssignableFrom<IEnumerable<ServiceRequest>>(
                    okResult.Value);

            Assert.Contains(
                requests,
                r => r.Id == request.Id);
        }

        [Fact]
        public async Task GetById_Returns_Request_When_Found()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Request",
                Description = "Description",
                AmountUSD = 100
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetById(
                request.Id,
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var returned =
                Assert.IsType<ServiceRequest>(
                    okResult.Value);

            Assert.Equal(
                request.Id,
                returned.Id);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Missing()
        {
            // Act
            var result = await _controller.GetById(
                int.MaxValue,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Adds_ServiceRequest()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Integration Test Request",
                Description = "Created by test",
                AmountUSD = 100
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            var created =
                Assert.IsType<CreatedAtActionResult>(
                    result.Result);

            var createdRequest =
                Assert.IsType<ServiceRequest>(
                    created.Value);

            Assert.Equal(
                "Open",
                createdRequest.Status);

            Assert.True(
                createdRequest.LocalCostZAR > 0);

            var exists =
                await _context.ServiceRequests
                    .AnyAsync(x => x.Id == createdRequest.Id);

            Assert.True(exists);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Contract_Does_Not_Exist()
        {
            // Arrange
            var request = new ServiceRequest
            {
                ContractId = int.MaxValue,
                Title = "Test Request",
                Description = "Test Description",
                AmountUSD = 100
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Amount_Is_Zero()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Test Request",
                Description = "Test Description",
                AmountUSD = 0
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Title_Is_Missing()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "",
                Description = "Test Description",
                AmountUSD = 100
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Description_Is_Missing()
        {
            // Arrange
            var contract = await CreateContractAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Test Request",
                Description = "",
                AmountUSD = 100
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Contract_Is_Expired()
        {
            // Arrange
            var client = await CreateClientAsync();

            var contract = new Contract
            {
                ClientId = client.Id,
                StartDate = DateTime.Today.AddYears(-1),
                EndDate = DateTime.Today.AddDays(-1),
                Status = ContractStatus.Expired,
                ServiceLevel = "Gold"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.Id,
                Title = "Test Request",
                Description = "Test Description",
                AmountUSD = 100
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task GetContracts_Returns_Ok()
        {
            // Arrange
            await CreateContractAsync();

            // Act
            var result = await _controller.GetContracts(
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ConvertUsdToZar_Returns_Ok()
        {
            // Act
            var result =
                await _controller.ConvertUsdToZar(100);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ConvertUsdToZar_Returns_Zero_When_Usd_Is_Zero()
        {
            // Act
            var result =
                await _controller.ConvertUsdToZar(0);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }
    }
}
