using GLMS.Core.Models;
using GLMS.Web.Data;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyService _currencyService;
        public ServiceRequestsController(ApplicationDbContext context, CurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // LIST ALL REQUESTS
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(p => p.Client)
                .ToListAsync();

            return View(requests);
        }

        // CREATE PAGE
        public IActionResult Create()
        {
            ViewBag.Contracts = new SelectList(
                _context.Contracts
                    .Include(c => c.Client)
                    .Select(c => new
                    {
                        c.Id,
                        Name = c.Client.Name
                    }),
                "Id",
                "Name");

            return View();
        }
        // CREATE REQUEST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("", "Parent contract not found.");
            }
            else if (contract.Status == ContractStatus.Expired ||
                     contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("",
                    "Cannot create Service Request. Contract is Expired or On Hold.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Contracts = new SelectList(
                    _context.Contracts.Include(c => c.Client),
                    "Id",
                    "Client.Name",
                    request.ContractId);

                return View(request);
            }

            request.CreatedDate = DateTime.UtcNow;

            // CALL EXTERNAL API
            var rate = await _currencyService.GetUsdToZarRate();

            // AUTO CALCULATE ZAR
            request.LocalCostZAR = request.AmountUSD * rate;

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(p => p.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> ConvertUsdToZar(decimal usd)
        {
            var rate = await _currencyService.GetUsdToZarRate();

            var zar = usd * rate;

            return Json(zar);
        }
    }
}