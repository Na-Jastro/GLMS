using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsApiController : ControllerBase
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsApiController> _logger;

        public ServiceRequestsApiController(
            IServiceRequestRepository serviceRequestRepository,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsApiController> logger)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: api/servicerequests
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetAll(
            CancellationToken cancellationToken)
        {
            try
            {
                var requests =
                    await _serviceRequestRepository
                        .GetAllAsync(cancellationToken);

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service requests.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving service requests.");
            }
        }

        // GET: api/servicerequests/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceRequest>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var request =
                    await _serviceRequestRepository
                        .GetDetailsAsync(
                            id,
                            cancellationToken);

                if (request == null)
                {
                    return NotFound(
                        $"Service Request with Id {id} was not found.");
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service request details. Id {RequestId}",
                    id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the service request.");
            }
        }

        // POST: api/servicerequests
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceRequest>> Create(
            [FromBody] ServiceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var contract =
                    await _serviceRequestRepository
                        .GetContractAsync(
                            request.ContractId,
                            cancellationToken);

                if (contract == null)
                {
                    return BadRequest(
                        "Parent contract not found.");
                }

                if (contract.Status == ContractStatus.Expired ||
                    contract.Status == ContractStatus.OnHold)
                {
                    return BadRequest(
                        "Cannot create Service Request. Contract is Expired or On Hold.");
                }

                if (request.AmountUSD <= 0)
                {
                    ModelState.AddModelError(
                        nameof(request.AmountUSD),
                        "Amount must be greater than zero.");
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    ModelState.AddModelError(
                        nameof(request.Title),
                        "Title is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    ModelState.AddModelError(
                        nameof(request.Description),
                        "Description is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.CreatedDate =
                    DateTime.UtcNow;

                request.Status =
                    "Open";

                // External Currency API
                var rate =
                    await _currencyService
                        .GetUsdToZarRate();

                request.LocalCostZAR =
                    request.AmountUSD * rate;

                await _serviceRequestRepository
                    .CreateAsync(
                        request,
                        cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = request.Id },
                    request);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating service request.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while creating the service request.");
            }
        }

        // GET: api/servicerequests/contracts
        [HttpGet("contracts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContracts(
            CancellationToken cancellationToken)
        {
            try
            {
                var contracts =
                    await _serviceRequestRepository
                        .GetContractsAsync();

                var result =
                    contracts.Select(c => new
                    {
                        c.Id,
                        Name = c.Client != null
                            ? c.Client.Name
                            : $"Contract #{c.Id}"
                    });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving contracts.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving contracts.");
            }
        }

        // GET: api/servicerequests/convert-usd-to-zar?usd=100
        [HttpGet("convert-usd-to-zar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConvertUsdToZar(
            [FromQuery] decimal usd)
        {
            try
            {
                if (usd <= 0)
                {
                    return Ok(new
                    {
                        USD = usd,
                        ZAR = 0
                    });
                }

                var rate =
                    await _currencyService
                        .GetUsdToZarRate();

                var zar =
                    usd * rate;

                return Ok(new
                {
                    USD = usd,
                    ExchangeRate = rate,
                    ZAR = zar
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error converting USD to ZAR.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while converting currency.");
            }
        }
    }
}
