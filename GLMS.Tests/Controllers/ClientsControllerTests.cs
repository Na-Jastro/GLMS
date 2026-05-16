using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GLMS.Tests.Controllers
{
    public class ClientsControllerTests
    {
        private readonly Mock<IClientRepository> _repositoryMock;
        private readonly Mock<ILogger<ClientsController>> _loggerMock;

        private readonly Mock<ISession> _sessionMock;
        private readonly Mock<HttpContext> _httpContextMock;

        private readonly ClientsController _controller;

        public ClientsControllerTests()
        {
            _repositoryMock =
                new Mock<IClientRepository>();

            _loggerMock =
                new Mock<ILogger<ClientsController>>();

            _sessionMock =
                new Mock<ISession>();

            _httpContextMock =
                new Mock<HttpContext>();

            // SESSION VALUE
            var sessionBytes =
                System.Text.Encoding.UTF8
                    .GetBytes("test@test.com");

            _sessionMock
                .Setup(s => s.TryGetValue(
                    "UserEmail",
                    out sessionBytes))
                .Returns(true);

            _httpContextMock
                .Setup(c => c.Session)
                .Returns(_sessionMock.Object);

            _controller = new ClientsController(
                _repositoryMock.Object,
                _loggerMock.Object);

            _controller.ControllerContext =
                new ControllerContext
                {
                    HttpContext =
                        _httpContextMock.Object
                };
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client
                {
                    Id = 1,
                    Name = "Client A"
                },
                new Client
                {
                    Id = 2,
                    Name = "Client B"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clients);

            // Act
            var result =
                await _controller.Index(
                    CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult =
                result as ViewResult;

            viewResult!.Model
                .Should()
                .BeEquivalentTo(clients);
        }

        [Fact]
        public async Task Details_ShouldReturnView_WhenClientExists()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Name = "Test Client"
            };

            _repositoryMock
                .Setup(r => r.GetDetailsAsync(
                    client.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result =
                await _controller.Details(
                    client.Id,
                    CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult =
                result as ViewResult;

            viewResult!.Model
                .Should()
                .Be(client);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenModelStateInvalid()
        {
            // Arrange
            var client = new Client();

            _controller.ModelState.AddModelError(
                "Name",
                "Required");

            // Act
            var result =
                await _controller.Create(
                    client,
                    CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult =
                result as ViewResult;

            viewResult!.Model
                .Should()
                .Be(client);
        }

        [Fact]
        public async Task Delete_Get_ShouldReturnView_WhenClientExists()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Name = "Delete Client"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    client.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result =
                await _controller.Delete(
                    client.Id,
                    CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult =
                result as ViewResult;

            viewResult!.Model
                .Should()
                .Be(client);
        }
    }
}