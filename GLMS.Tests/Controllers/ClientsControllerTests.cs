using GLMS.Api.Controllers;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure;
using GLMS.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GLMS.Tests.Controllers
{
    public class ClientsApiControllerTests
    {
        private readonly GLMSDbContext _context;
        private readonly ClientsApiController _controller;

        public ClientsApiControllerTests()
        {
            var options = new DbContextOptionsBuilder<GLMSDbContext>()
                .UseSqlServer(
                    "server=Justice;database=GLMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;")
                .Options;

            _context = new GLMSDbContext(options);

            // Create database if it doesn't exist
            _context.Database.EnsureCreated();

            IClientRepository repository = new ClientRepository(_context);

            _controller = new ClientsApiController(
                repository,
                NullLogger<ClientsApiController>.Instance);
        }

        [Fact]
        public async Task GetAll_Returns_All_Clients()
        {
            // Arrange
            var client1 = new Client { Name = $"Client-{Guid.NewGuid()}", ContactDetails = "Test Contact Details", Region = "Test Region" };
            var client2 = new Client { Name = $"Client-{Guid.NewGuid()}", ContactDetails = "Test Contact Details 2", Region = "Test Region 2" };

            _context.Clients.AddRange(client1, client2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var clients =
                Assert.IsAssignableFrom<IEnumerable<Client>>(okResult.Value);

            Assert.Contains(clients, c => c.Id == client1.Id);
            Assert.Contains(clients, c => c.Id == client2.Id);
        }

        [Fact]
        public async Task GetById_Returns_Client_When_Found()
        {
            // Arrange
            var client = new Client
            {
                Name = $"Test-{Guid.NewGuid()}",
                ContactDetails = "Test Contact Details",
                Region = "Test Region"
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetById(
                client.Id,
                CancellationToken.None);

            // Assert
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var returnedClient =
                Assert.IsType<Client>(okResult.Value);

            Assert.Equal(client.Id, returnedClient.Id);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Client_Does_Not_Exist()
        {
            // Act
            var result = await _controller.GetById(
                int.MaxValue,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_Adds_Client()
        {
            // Arrange
            var client = new Client
            {
                Name = $"NewClient-{Guid.NewGuid()}",
                ContactDetails = "New Client Contact Details",
                Region = "New Client Region"
            };

            // Act
            var result = await _controller.Create(
                client,
                CancellationToken.None);

            // Assert
            var created =
                Assert.IsType<CreatedAtActionResult>(result.Result);

            var createdClient =
                Assert.IsType<Client>(created.Value);

            var exists = await _context.Clients
                .AnyAsync(c => c.Id == createdClient.Id);

            Assert.True(exists);
        }

        [Fact]
        public async Task Update_Returns_NoContent()
        {
            // Arrange
            var client = new Client
            {
                Name = $"Old-{Guid.NewGuid()}",
                ContactDetails = "Old Contact Details",
                Region = "Old Region"
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            client.Name = "Updated Client";

            // Act
            var result = await _controller.Update(
                client.Id,
                client,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updated =
                await _context.Clients.FindAsync(client.Id);

            Assert.Equal("Updated Client", updated!.Name);
        }

        [Fact]
        public async Task Delete_Removes_Client()
        {
            // Arrange
            var client = new Client
            {
                Name = $"Delete-{Guid.NewGuid()}",
                ContactDetails = "Delete Contact Details",
                Region = "Delete Region"
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(
                client.Id,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var deleted =
                await _context.Clients.FindAsync(client.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Client_Does_Not_Exist()
        {
            // Act
            var result = await _controller.Delete(
                int.MaxValue,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}