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
            _repositoryMock = new Mock<IContractRepository>();

            _serviceMock = new Mock<IContractService>();

            _environmentMock = new Mock<IWebHostEnvironment>();

            _loggerMock = new Mock<ILogger<ContractsController>>();

            _controller = new ContractsController(
                _repositoryMock.Object,
                _serviceMock.Object,
                _environmentMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task Index_ShouldReturnViewWithContracts()
        {
            // Arrange
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
            var result = await _controller.Index(
                null,
                null,
                null,
                null,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var view = result as ViewResult;

            view!.Model.Should().BeEquivalentTo(contracts);
        }


        [Fact]
        public async Task Details_ShouldReturnView_WhenContractExists()
        {
            // Arrange
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
            var result = await _controller.Details(
                1,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var view = result as ViewResult;

            view!.Model.Should().Be(contract);
        }

       

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenInvalid()
        {
            // Arrange
            var contract = new Contract
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = await _controller.Create(
                contract,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Edit_Get_ShouldReturnView_WhenExists()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

            // Act
            var result = await _controller.Edit(
                1,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }


        [Fact]
        public async Task Delete_ShouldReturnView_WhenExists()
        {
            // Arrange
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
            var result = await _controller.Delete(
                1,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
        [Theory]
        //[InlineData("SignedAgreement.pdf", true)]
        [InlineData("SignedAgreement.docx", false)]
        public async Task UploadSignedAgreement_ShouldOnlyAllowPdfFiles(
    string fileName,
    bool shouldPass)
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(contract);

            _environmentMock
                .Setup(e => e.WebRootPath)
                .Returns(Path.GetTempPath());

            var filePath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Documents",
                    fileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read);

            IFormFile file =
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

            _controller.TempData =
                new TempDataDictionary(
                    new DefaultHttpContext(),
                    Mock.Of<ITempDataProvider>());

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
                _controller.TempData["Success"]
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
                _controller.TempData["Error"]
                    .Should()
                    .Be("Only PDF files are allowed.");

                _repositoryMock.Verify(
                    r => r.UpdateAsync(
                        It.IsAny<Contract>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
            }
        }

        [Fact]
        public async Task DownloadAgreement_ShouldReturnNotFound_WhenContractMissing()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contract?)null);

            // Act
            var result = await _controller.DownloadAgreement(
                1,
                CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

       
    }
}