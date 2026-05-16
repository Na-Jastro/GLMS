// Controllers/ClientsController.cs

using GLMS.Core.Models;
using GLMS.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(
            IClientRepository clientRepository,
            ILogger<ClientsController> logger)
        {
            _clientRepository = clientRepository;
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

        // GET: Clients
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var clients = await _clientRepository
                    .GetAllAsync(cancellationToken);

                return View(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while retrieving clients.");

                TempData["ErrorMessage"] =
                    "An error occurred while loading clients.";

                return View(new List<Client>());
            }
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var client = await _clientRepository
                    .GetDetailsAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["ErrorMessage"] =
                        "Client not found.";

                    return NotFound();
                }

                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while retrieving client details for Id {ClientId}.",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading client details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Client client,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (!ModelState.IsValid)
                    return View(client);

                await _clientRepository
                    .CreateAsync(client, cancellationToken);

                TempData["SuccessMessage"] =
                    "Client created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating client.");

                TempData["ErrorMessage"] =
                    "An error occurred while creating the client.";

                return View(client);
            }
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var client = await _clientRepository
                    .GetByIdAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["ErrorMessage"] =
                        "Client not found.";

                    return NotFound();
                }

                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while loading client for editing. Id: {ClientId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading the client.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Client client,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                if (id != client.Id)
                {
                    TempData["ErrorMessage"] =
                        "Invalid client request.";

                    return BadRequest();
                }

                if (!ModelState.IsValid)
                    return View(client);

                await _clientRepository
                    .UpdateAsync(client, cancellationToken);

                TempData["SuccessMessage"] =
                    "Client updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Client not found for update. Id: {ClientId}",
                    id);

                TempData["ErrorMessage"] =
                    "Client not found.";

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while updating client. Id: {ClientId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while updating the client.";

                return View(client);
            }
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var auth = CheckLogin();

            if (auth != null)
                return auth;

            try
            {
                var client = await _clientRepository
                    .GetByIdAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["ErrorMessage"] =
                        "Client not found.";

                    return NotFound();
                }

                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while loading client for deletion. Id: {ClientId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while loading the client.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Clients/Delete/5
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
                var deleted = await _clientRepository
                    .DeleteAsync(id, cancellationToken);

                if (!deleted)
                {
                    TempData["ErrorMessage"] =
                        "Client not found.";

                    return NotFound();
                }

                TempData["SuccessMessage"] =
                    "Client deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while deleting client. Id: {ClientId}",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while deleting the client.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}