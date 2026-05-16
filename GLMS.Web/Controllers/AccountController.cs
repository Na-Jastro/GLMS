using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly GLMSDbContext _context;
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;

        public AccountController(
            GLMSDbContext context,
            IContractRepository contractRepository,
            IClientRepository clientRepository)
        {
            _context = context;
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
        }

        // REGISTER
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(user);
        }

        // LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == email &&
                    x.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Message = "Invalid Email or Password";

            return View();
        }

        // DASHBOARD
        public async Task<IActionResult> Dashboard(
     CancellationToken cancellationToken)
        {
            var email =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Email = email;

            // CONTRACT STATS
            ViewBag.TotalContracts =
                await _contractRepository
                    .GetTotalCountAsync(cancellationToken);

            ViewBag.ActiveContracts =
                await _contractRepository
                    .GetActiveCountAsync(cancellationToken);

            ViewBag.ExpiredContracts =
                await _contractRepository
                    .GetExpiredCountAsync(cancellationToken);

            // CLIENT STATS
            var clients =
                await _clientRepository
                    .GetAllAsync(cancellationToken);

            ViewBag.TotalClients =
                clients.Count();

            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
