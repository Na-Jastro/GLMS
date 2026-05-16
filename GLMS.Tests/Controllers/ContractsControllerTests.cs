using FluentAssertions;
using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using GLMS.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace GLMS.Tests.Controllers
{
    public class ContractsControllerTests
    {
        private readonly Mock<IContractRepository> _repositoryMock;
        private readonly Mock<IContractService> _serviceMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<ILogger<ContractsController>> _loggerMock;

        private readonly ContractsController _controller;

        public ContractsControllerTests()
        {
            _repositoryMock =
                new Mock<IContractRepository>();

            _serviceMock =
                new Mock<IContractService>();

            _environmentMock =
                new Mock<IWebHostEnvironment>();

            _loggerMock =
                new Mock<ILogger<ContractsController>>();

            _controller = new ContractsController(
                _repositoryMock.Object,
                _serviceMock.Object,
                _environmentMock.Object,
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
        public async Task Index_ShouldReturnViewWithContracts()
        {
            // Arrange
            SetupLoggedInUser();

            var contracts = new List<Contract>
            {
                new Contract
                {
                    Id = 1,
                    ServiceLevel = "Gold",
                    Status = ContractStatus.Active
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync(
                    null,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contracts);

            _repositoryMock
                .Setup(r => r.GetClientsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Client>());

            _repositoryMock
                .Setup(r => r.GetTotalCountAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _repositoryMock
                .Setup(r => r.GetActiveCountAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _repositoryMock
                .Setup(r => r.GetExpiredCountAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            _serviceMock
                .Setup(s => s.AutoUpdateExpiryAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result =
                await _controller.Index(
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();

            var view =
                result as ViewResult;

            view!.Model
                .Should()
                .BeEquivalentTo(contracts);
        }

        [Fact]
        public async Task Details_ShouldReturnView_WhenContractExists()
        {
            // Arrange
            SetupLoggedInUser();

            var contract = new Contract
            {
                Id = 1,
                ServiceLevel = "Gold"
            };

            _repositoryMock
                .Setup(r => r.GetDetailsAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

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
                .Be(contract);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenInvalid()
        {
            // Arrange
            SetupLoggedInUser();

            var contract = new Contract
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(-1)
            };

            _repositoryMock
                .Setup(r => r.GetClientsAsync())
                .ReturnsAsync(new List<Client>());

            // Act
            var result =
                await _controller.Create(
                    contract,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Edit_Get_ShouldReturnView_WhenExists()
        {
            // Arrange
            SetupLoggedInUser();

            var contract = new Contract
            {
                Id = 1
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

            _repositoryMock
                .Setup(r => r.GetClientsAsync())
                .ReturnsAsync(new List<Client>());

            // Act
            var result =
                await _controller.Edit(
                    1,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnView_WhenExists()
        {
            // Arrange
            SetupLoggedInUser();

            var contract = new Contract
            {
                Id = 1
            };

            _repositoryMock
                .Setup(r => r.GetDetailsAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

            // Act
            var result =
                await _controller.Delete(
                    1,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<ViewResult>();
        }

        [Theory]
        [InlineData("SignedAgreement.pdf", true)]
        [InlineData("SignedAgreement.docx", false)]
        public async Task UploadSignedAgreement_ShouldOnlyAllowPdfFiles(
           string fileName,
           bool shouldPass)
        {
            // Arrange
            SetupLoggedInUser();

            var contract = new Contract
            {
                Id = 1
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

            var tempRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempRoot);

            _environmentMock
                .Setup(e => e.WebRootPath)
                .Returns(tempRoot);

            // CREATE TEMP FILE
            var tempFile =
                Path.Combine(
                    tempRoot,
                    fileName);

            await File.WriteAllTextAsync(
                tempFile,
                "Test Content");

            IFormFile file;

            await using (var stream =
                new FileStream(
                    tempFile,
                    FileMode.Open,
                    FileAccess.Read))
            {
                file =
                    new FormFile(
                        stream,
                        0,
                        stream.Length,
                        "file",
                        fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType =
                            fileName.EndsWith(".pdf")
                                ? "application/pdf"
                                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    };

                // Act
                var result =
                    await _controller.UploadSignedAgreement(
                        1,
                        file,
                        CancellationToken.None);

                // Assert
                result.Should()
                    .BeOfType<RedirectToActionResult>();

                if (shouldPass)
                {
                    _controller.TempData["SuccessMessage"]
                        .Should()
                        .Be("Signed agreement uploaded successfully.");

                    contract.SignedAgreementPath
                        .Should()
                        .NotBeNullOrWhiteSpace();

                    _repositoryMock.Verify(
                        r => r.UpdateAsync(
                            contract,
                            It.IsAny<CancellationToken>()),
                        Times.Once);
                }
                else
                {
                    _controller.TempData["ErrorMessage"]
                        .Should()
                        .Be("Only PDF files are allowed.");

                    _repositoryMock.Verify(
                        r => r.UpdateAsync(
                            It.IsAny<Contract>(),
                            It.IsAny<CancellationToken>()),
                        Times.Never);
                }
            }

            // CLEANUP
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(
                    tempRoot,
                    true);
            }
        }
        [Fact]
        public async Task DownloadAgreement_ShouldRedirect_WhenContractMissing()
        {
            // Arrange
            SetupLoggedInUser();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contract?)null);

            // Act
            var result =
                await _controller.DownloadAgreement(
                    1,
                    CancellationToken.None);

            // Assert
            result.Should()
                .BeOfType<RedirectToActionResult>();
        }
    }
}