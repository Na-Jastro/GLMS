using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using GLMS.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GLMS.Tests.Controllers
{
    public class ServiceRequestsControllerTests
    {
        private readonly Mock<IServiceRequestRepository> _repositoryMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<ILogger<ServiceRequestsController>> _loggerMock;

        private readonly ServiceRequestsController _controller;

        public ServiceRequestsControllerTests()
        {
            _repositoryMock = new Mock<IServiceRequestRepository>();

            _currencyServiceMock = new Mock<ICurrencyService>();

            _loggerMock = new Mock<ILogger<ServiceRequestsController>>();

            _controller = new ServiceRequestsController(
                _repositoryMock.Object,
                _currencyServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnView()
        {
            // Arrange
            var requests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1,
                    Title = "Request"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(requests);

            // Act
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Details_ShouldReturnView_WhenExists()
        {
            // Arrange
            var request = new ServiceRequest
            {
                Id = 1,
                Title = "Request"
            };

            _repositoryMock
                .Setup(r => r.GetDetailsAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act
            var result = await _controller.Details(
                1,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }


        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenInvalid()
        {
            // Arrange
            var request = new ServiceRequest
            {
                AmountUSD = -1
            };

            // Act
            var result = await _controller.Create(
                request,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        

        [Fact]
        public async Task ConvertUsdToZar_ShouldReturnJson()
        {
            // Arrange
            _currencyServiceMock
                .Setup(c => c.GetUsdToZarRate())
                .ReturnsAsync(18);

            // Act
            var result = await _controller.ConvertUsdToZar(100);

            // Assert
            result.Should().BeOfType<JsonResult>();

            var json = result as JsonResult;

            json!.Value.Should().Be(1800);
        }
    }
}