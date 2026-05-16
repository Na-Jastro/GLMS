using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            IServiceRequestRepository serviceRequestRepository,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _currencyService = currencyService;
            _logger = logger;
        }

        // LOGIN CHECK
        private IActionResult? CheckLogin()
        {
            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] =
                    "Please login first.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            return null;
        }

        // LOAD CONTRACTS DROPDOWN
        private async Task LoadContractsDropdown(
            int? selectedContract = null)
        {
            var contracts = await _serviceRequestRepository
                .GetContractsAsync();

            ViewBag.Contracts = new SelectList(
                contracts.Select(c => new
                {
                    c.Id,
                    Name = c.Client != null
                        ? c.Client.Name
                        : $"Contract #{c.Id}"
                }),
                "Id",
                "Name",
                selectedContract);
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var requests = await _serviceRequestRepository
                    .GetAllAsync(cancellationToken);

                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service requests.");

                TempData["ErrorMessage"] =
                    "An error occurred while loading service requests.";

                return View(new List<ServiceRequest>());
            }
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            await LoadContractsDropdown();

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceRequest request,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var contract = await _serviceRequestRepository
                    .GetContractAsync(
                        request.ContractId,
                        cancellationToken);

                if (contract == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Parent contract not found.");
                }
                else if (contract.Status == ContractStatus.Expired ||
                         contract.Status == ContractStatus.OnHold)
                {
                    ModelState.AddModelError(
                        "",
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
                    await LoadContractsDropdown(
                        request.ContractId);

                    return View(request);
                }

                request.CreatedDate = DateTime.UtcNow;

                request.Status = "Open";

                // EXTERNAL API
                var rate = await _currencyService
                    .GetUsdToZarRate();

                request.LocalCostZAR =
                    request.AmountUSD * rate;

                await _serviceRequestRepository
                    .CreateAsync(
                        request,
                        cancellationToken);

                TempData["SuccessMessage"] =
                    "Service request created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating service request.");

                TempData["ErrorMessage"] =
                    "An error occurred while creating service request.";

                await LoadContractsDropdown(
                    request.ContractId);

                return View(request);
            }
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var request = await _serviceRequestRepository
                    .GetDetailsAsync(
                        id,
                        cancellationToken);

                if (request == null)
                {
                    TempData["ErrorMessage"] =
                        "Service request not found.";

                    return RedirectToAction(nameof(Index));
                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service request details. Id {RequestId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading service request details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX USD -> ZAR
        [HttpGet]
        public async Task<IActionResult> ConvertUsdToZar(
            decimal usd)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (usd <= 0)
                {
                    return Json(0);
                }

                var rate = await _currencyService
                    .GetUsdToZarRate();

                var zar = usd * rate;

                return Json(zar);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error converting USD to ZAR.");

                return Json(0);
            }
        }
    }
}
