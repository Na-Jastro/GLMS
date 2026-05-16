using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using GLMS.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

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
            _repositoryMock =
                new Mock<IServiceRequestRepository>();

            _currencyServiceMock =
                new Mock<ICurrencyService>();

            _loggerMock =
                new Mock<ILogger<ServiceRequestsController>>();

            _controller =
                new ServiceRequestsController(
                    _repositoryMock.Object,
                    _currencyServiceMock.Object,
                    _loggerMock.Object);
        }

        // HELPER
        private void SetupLoggedInUser()
        {
            var sessionMock =
                new Mock<ISession>();

            var context =
                new DefaultHttpContext();

            var sessionBytes =
                Encoding.UTF8.GetBytes("test@test.com");

            sessionMock
                .Setup(s => s.TryGetValue(
                    "UserEmail",
                    out sessionBytes))
                .Returns(true);

            context.Session =
                sessionMock.Object;

            _controller.ControllerContext =
                new ControllerContext
                {
                    HttpContext = context
                };

            _controller.TempData =
                new TempDataDictionary(
                    context,
                    Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Index_ShouldReturnView()
        {
            // Arrange
            SetupLoggedInUser();

            var requests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1,
                    Title = "Request"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(requests);

            // Act
            var result =
                await _controller.Index(
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();

            var view =
                result as ViewResult;

            view!.Model
                .Should()
                .BeEquivalentTo(requests);
        }

        [Fact]
        public async Task Details_ShouldReturnView_WhenExists()
        {
            // Arrange
            SetupLoggedInUser();

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
            var result =
                await _controller.Details(
                    1,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();

            var view =
                result as ViewResult;

            view!.Model
                .Should()
                .Be(request);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenInvalid()
        {
            // Arrange
            SetupLoggedInUser();

            var request = new ServiceRequest
            {
                AmountUSD = -1
            };

            _repositoryMock
                .Setup(r => r.GetContractsAsync())
                .ReturnsAsync(new List<Contract>());

            // Act
            var result =
                await _controller.Create(
                    request,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();
        }

        [Fact]
        public async Task ConvertUsdToZar_ShouldReturnJson()
        {
            // Arrange
            SetupLoggedInUser();

            _currencyServiceMock
                .Setup(c => c.GetUsdToZarRate())
                .ReturnsAsync(18);

            // Act
            var result =
                await _controller.ConvertUsdToZar(100);

            // Assert
            result.Should()
                .BeOfType<JsonResult>();

            var json =
                result as JsonResult;

            json!.Value
                .Should()
                .Be(1800m);
        }
    }
}