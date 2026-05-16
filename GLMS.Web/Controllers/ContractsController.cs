using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractService _service;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractRepository contractRepository,
            IContractService service,
            IWebHostEnvironment environment,
            ILogger<ContractsController> logger)
        {
            _contractRepository = contractRepository;
            _service = service;
            _environment = environment;
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

        // LOAD DROPDOWNS
        private async Task LoadDropdowns(
            int? selectedClient = null)
        {
            var clients = await _contractRepository
                .GetClientsAsync();

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
                await _service.AutoUpdateExpiryAsync();

                var contracts =
                    await _contractRepository.GetAllAsync(
                        start,
                        end,
                        status,
                        clientId,
                        cancellationToken);

                ViewBag.Clients = new SelectList(
                    await _contractRepository
                        .GetClientsAsync(cancellationToken),
                    "Id",
                    "Name");

                ViewBag.Total =
                    await _contractRepository
                        .GetTotalCountAsync(cancellationToken);

                ViewBag.Active =
                    await _contractRepository
                        .GetActiveCountAsync(cancellationToken);

                ViewBag.Expired =
                    await _contractRepository
                        .GetExpiredCountAsync(cancellationToken);

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
                    await _contractRepository
                        .GetDetailsAsync(
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

                await _contractRepository
                    .CreateAsync(
                        contract,
                        cancellationToken);

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
                    await _contractRepository
                        .GetByIdAsync(
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

                await _contractRepository
                    .UpdateAsync(
                        contract,
                        cancellationToken);

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
                    await _contractRepository
                        .GetDetailsAsync(
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
                var deleted =
                    await _contractRepository
                        .DeleteAsync(
                            id,
                            cancellationToken);

                if (!deleted)
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

                var extension =
                    Path.GetExtension(file.FileName)
                        .ToLower();

                if (extension != ".pdf")
                {
                    TempData["ErrorMessage"] =
                        "Only PDF files are allowed.";

                    return RedirectToAction(nameof(Index));
                }

                var contract =
                    await _contractRepository
                        .GetByIdAsync(
                            id,
                            cancellationToken);

                if (contract == null)
                {
                    TempData["ErrorMessage"] =
                        "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                var uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "agreements");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName =
                    $"contract_{id}_{Guid.NewGuid()}.pdf";

                var filePath = Path.Combine(
                    uploadFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(
                        stream,
                        cancellationToken);
                }

                contract.SignedAgreementPath =
                    $"/agreements/{fileName}";

                await _contractRepository
                    .UpdateAsync(
                        contract,
                        cancellationToken);

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
                var contract =
                    await _contractRepository
                        .GetByIdAsync(
                            id,
                            cancellationToken);

                if (contract == null ||
                    string.IsNullOrEmpty(
                        contract.SignedAgreementPath))
                {
                    TempData["ErrorMessage"] =
                        "Agreement not found.";

                    return RedirectToAction(nameof(Index));
                }

                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    contract.SignedAgreementPath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] =
                        "Agreement file not found.";

                    return RedirectToAction(nameof(Index));
                }

                var bytes =
                    await System.IO.File
                        .ReadAllBytesAsync(
                            filePath,
                            cancellationToken);

                return File(
                    bytes,
                    "application/pdf",
                    "SignedAgreement.pdf");
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

