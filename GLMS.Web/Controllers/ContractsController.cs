using GLMS.Core.Models;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ContractService _service;
        private readonly IWebHostEnvironment _environment;

        public ContractsController(
            ApplicationDbContext context,
            ContractService service,
            IWebHostEnvironment environment)
        {
            _context = context;
            _service = service;
            _environment = environment;
        }

        private void LoadDropdowns(int? selectedClient = null)
        {
            ViewBag.Clients = new SelectList(
                _context.Clients.OrderBy(c => c.Name),
                "Id",
                "Name",
                selectedClient);

            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(ContractStatus)));
        }

        // SEARCH / FILTER USING LINQ
        public async Task<IActionResult> Index(
            DateTime? start,
            DateTime? end,
            ContractStatus? status,
            int? clientId)
        {
            await _service.AutoUpdateExpiryAsync();

            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (start.HasValue)
                query = query.Where(c => c.StartDate >= start.Value);

            if (end.HasValue)
                query = query.Where(c => c.EndDate <= end.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (clientId.HasValue)
                query = query.Where(c => c.ClientId == clientId.Value);

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");

            ViewBag.Total = await _context.Contracts.CountAsync();
            ViewBag.Active = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            ViewBag.Expired = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Expired);

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(contract.ClientId);
                return View(contract);
            }

            _context.Add(contract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            LoadDropdowns(contract.ClientId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(contract.ClientId);
                return View(contract);
            }

            _context.Update(contract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            _context.Remove(contract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadSignedAgreement(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file selected.";
                return RedirectToAction(nameof(Index));
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction(nameof(Index));
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                TempData["Error"] = "Contract not found.";
                return RedirectToAction(nameof(Index));
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "agreements");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = $"contract_{id}_{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            contract.SignedAgreementPath = $"/agreements/{fileName}";
            _context.Update(contract);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Signed agreement uploaded successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound("Agreement not found.");

            var filePath = Path.Combine(
                _environment.WebRootPath,
                contract.SignedAgreementPath.TrimStart('/'));

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", "SignedAgreement.pdf");
        }
    }
}