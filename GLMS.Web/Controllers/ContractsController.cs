using GLMS.Core.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractApiService _contractApiService;
        private readonly IClientService _clientService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractApiService contractApiService,
            IClientService clientService,
            ILogger<ContractsController> logger)
        {
            _contractApiService = contractApiService;
            _clientService = clientService;
            _logger = logger;
        }

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

        private async Task LoadDropdowns(
            int? selectedClient = null)
        {
            var clients =
                await _clientService.GetAllAsync();

            ViewBag.Clients = new SelectList(
                clients,
                "Id",
                "Name",
                selectedClient);

            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(ContractStatus)));
        }

        // GET: Contracts
        public async Task<IActionResult> Index(
            DateTime? start,
            DateTime? end,
            ContractStatus? status,
            int? clientId,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var contracts =
                    await _contractApiService.GetAllAsync(
                        start,
                        end,
                        status,
                        clientId,
                        cancellationToken);

                ViewBag.Clients = new SelectList(
                    await _clientService.GetAllAsync(
                        cancellationToken),
                    "Id",
                    "Name");

                var statistics =
                    await _contractApiService.GetStatisticsAsync(
                        cancellationToken);

                ViewBag.Total =
                    statistics?.Total ?? 0;

                ViewBag.Active =
                    statistics?.Active ?? 0;

                ViewBag.Expired =
                    statistics?.Expired ?? 0;

                return View(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading contracts.");

                TempData["ErrorMessage"] =
                    "An error occurred while loading contracts.";

                return View(new List<Contract>());
            }
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var contract =
                    await _contractApiService.GetByIdAsync(
                        id,
                        cancellationToken);

                if (contract == null)
                {
                    TempData["ErrorMessage"] =
                        "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading contract details for Id {ContractId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading contract details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            await LoadDropdowns();

            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Contract contract,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (contract.EndDate < contract.StartDate)
                {
                    ModelState.AddModelError(
                        nameof(contract.EndDate),
                        "End date cannot be before start date.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdowns(contract.ClientId);

                    return View(contract);
                }

                var success =
                    await _contractApiService.CreateAsync(
                        contract,
                        cancellationToken);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "Failed to create contract.";

                    await LoadDropdowns(contract.ClientId);

                    return View(contract);
                }

                TempData["SuccessMessage"] =
                    "Contract created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating contract.");

                TempData["ErrorMessage"] =
                    "An error occurred while creating contract.";

                await LoadDropdowns(contract.ClientId);

                return View(contract);
            }
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var contract =
                    await _contractApiService.GetByIdAsync(
                        id,
                        cancellationToken);

                if (contract == null)
                {
                    TempData["ErrorMessage"] =
                        "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                await LoadDropdowns(contract.ClientId);

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading contract for edit. Id {ContractId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading contract.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Contract contract,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (id != contract.Id)
                {
                    TempData["ErrorMessage"] =
                        "Invalid contract.";

                    return RedirectToAction(nameof(Index));
                }

                if (contract.EndDate < contract.StartDate)
                {
                    ModelState.AddModelError(
                        nameof(contract.EndDate),
                        "End date cannot be before start date.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdowns(contract.ClientId);

                    return View(contract);
                }

                var success =
                    await _contractApiService.UpdateAsync(
                        contract,
                        cancellationToken);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "Failed to update contract.";

                    await LoadDropdowns(contract.ClientId);

                    return View(contract);
                }

                TempData["SuccessMessage"] =
                    "Contract updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating contract. Id {ContractId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while updating contract.";

                await LoadDropdowns(contract.ClientId);

                return View(contract);
            }
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var contract =
                    await _contractApiService.GetByIdAsync(
                        id,
                        cancellationToken);

                if (contract == null)
                {
                    TempData["ErrorMessage"] =
                        "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading contract for delete. Id {ContractId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading contract.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var success =
                    await _contractApiService.DeleteAsync(
                        id,
                        cancellationToken);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] =
                    "Contract deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting contract. Id {ContractId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while deleting contract.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contracts/UploadSignedAgreement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignedAgreement(
            int id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] =
                        "No file selected.";

                    return RedirectToAction(nameof(Index));
                }

                var success =
                    await _contractApiService.UploadAgreementAsync(
                        id,
                        file,
                        cancellationToken);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "Failed to upload agreement.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] =
                    "Signed agreement uploaded successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error uploading agreement.");

                TempData["ErrorMessage"] =
                    "An error occurred while uploading agreement.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var bytes =
                    await _contractApiService
                        .DownloadAgreementAsync(
                            id,
                            cancellationToken);

                if (bytes == null)
                {
                    TempData["ErrorMessage"] =
                        "Agreement not found.";

                    return RedirectToAction(nameof(Index));
                }

                return File(
                    bytes,
                    "application/pdf",
                    $"Contract_{id}_Agreement.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error downloading agreement.");

                TempData["ErrorMessage"] =
                    "An error occurred while downloading agreement.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}