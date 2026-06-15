using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;


namespace GLMS.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsApiController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContractsApiController> _logger;

        public ContractsApiController(
            IContractRepository contractRepository,
            IWebHostEnvironment environment,
            ILogger<ContractsApiController> logger)
        {
            _contractRepository = contractRepository;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/contracts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetAll(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] ContractStatus? status,
            [FromQuery] int? clientId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _contractRepository.AutoUpdateExpiryAsync();

                var contracts =
                    await _contractRepository.GetAllAsync(
                        start,
                        end,
                        status,
                        clientId,
                        cancellationToken);

                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error loading contracts.");

                return StatusCode(500,
                    "An error occurred while retrieving contracts.");
            }
        }

        // GET: api/contracts/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Contract>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var contract =
                    await _contractRepository.GetDetailsAsync(
                        id,
                        cancellationToken);

                if (contract == null)
                    return NotFound();

                return Ok(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving contract {ContractId}",
                    id);

                return StatusCode(500,
                    "An error occurred while retrieving the contract.");
            }
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<ActionResult<Contract>> Create(
            [FromBody] Contract contract,
            CancellationToken cancellationToken)
        {
            try
            {
                if (contract.EndDate < contract.StartDate)
                {
                    ModelState.AddModelError(
                        nameof(contract.EndDate),
                        "End date cannot be before start date.");
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _contractRepository.CreateAsync(
                    contract,
                    cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = contract.Id },
                    contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating contract.");

                return StatusCode(500,
                    "An error occurred while creating the contract.");
            }
        }

        // PUT: api/contracts/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] Contract contract,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != contract.Id)
                    return BadRequest(
                        "Route Id does not match Contract Id.");

                if (contract.EndDate < contract.StartDate)
                {
                    ModelState.AddModelError(
                        nameof(contract.EndDate),
                        "End date cannot be before start date.");
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _contractRepository.UpdateAsync(
                    contract,
                    cancellationToken);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating contract {ContractId}",
                    id);

                return StatusCode(500,
                    "An error occurred while updating the contract.");
            }
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var deleted =
                    await _contractRepository.DeleteAsync(
                        id,
                        cancellationToken);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting contract {ContractId}",
                    id);

                return StatusCode(500,
                    "An error occurred while deleting the contract.");
            }
        }

        // GET: api/contracts/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            CancellationToken cancellationToken)
        {
            try
            {
                var result = new
                {
                    Total = await _contractRepository
                        .GetTotalCountAsync(cancellationToken),

                    Active = await _contractRepository
                        .GetActiveCountAsync(cancellationToken),

                    Expired = await _contractRepository
                        .GetExpiredCountAsync(cancellationToken)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving statistics.");

                return StatusCode(500,
                    "An error occurred while retrieving statistics.");
            }
        }

        // GET: api/contracts/clients
        [HttpGet("clients")]
        public async Task<IActionResult> GetClients(
            CancellationToken cancellationToken)
        {
            try
            {
                var clients =
                    await _contractRepository
                        .GetClientsAsync(cancellationToken);

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving clients.");

                return StatusCode(500,
                    "An error occurred while retrieving clients.");
            }
        }

        // POST: api/contracts/{id}/upload-agreement
        [HttpPost("{id:int}/upload-agreement")]
        public async Task<IActionResult> UploadAgreement(
            int id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file selected.");

                var extension =
                    Path.GetExtension(file.FileName)
                        .ToLower();

                if (extension != ".pdf")
                    return BadRequest(
                        "Only PDF files are allowed.");

                var contract =
                    await _contractRepository.GetByIdAsync(
                        id,
                        cancellationToken);

                if (contract == null)
                    return NotFound("Contract not found.");

                var uploadFolder = Path.Combine(
                    _environment.ContentRootPath,
                    "agreements");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName =
                    $"contract_{id}_{Guid.NewGuid()}.pdf";

                var filePath =
                    Path.Combine(uploadFolder, fileName);

                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await file.CopyToAsync(
                        stream,
                        cancellationToken);
                }

                contract.SignedAgreementPath =
                    $"/agreements/{fileName}";

                await _contractRepository.UpdateAsync(
                    contract,
                    cancellationToken);

                return Ok(new
                {
                    Message =
                        "Agreement uploaded successfully.",
                    Path = contract.SignedAgreementPath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error uploading agreement.");

                return StatusCode(500,
                    "An error occurred while uploading the agreement.");
            }
        }

        // GET: api/contracts/{id}/download-agreement
        [HttpGet("{id:int}/download-agreement")]
        public async Task<IActionResult> DownloadAgreement(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var contract =
                    await _contractRepository.GetByIdAsync(
                        id,
                        cancellationToken);

                if (contract == null ||
                    string.IsNullOrEmpty(
                        contract.SignedAgreementPath))
                {
                    return NotFound(
                        "Agreement not found.");
                }

                var filePath = Path.Combine(
                    _environment.ContentRootPath,
                    contract.SignedAgreementPath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                    return NotFound(
                        "Agreement file not found.");

                var bytes =
                    await System.IO.File.ReadAllBytesAsync(
                        filePath,
                        cancellationToken);

                return File(
                    bytes,
                    "application/pdf",
                    $"Contract_{id}_Agreement.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error downloading agreement.");

                return StatusCode(500,
                    "An error occurred while downloading the agreement.");
            }
        }
    }
}
