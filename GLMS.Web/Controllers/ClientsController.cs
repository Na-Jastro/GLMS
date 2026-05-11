using GLMS.Core.Models;
using GLMS.Core.Repositories;
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

        // GET: Clients
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
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

                TempData["Error"] =
                    "An error occurred while loading clients.";

                return View(new List<Client>());
            }
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository
                    .GetDetailsAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
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

                TempData["Error"] =
                    "An error occurred while loading client details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Client client,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(client);

                await _clientRepository
                    .CreateAsync(client, cancellationToken);

                TempData["Success"] =
                    "Client created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating client.");

                TempData["Error"] =
                    "An error occurred while creating the client.";

                return View(client);
            }
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository
                    .GetByIdAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
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

                TempData["Error"] =
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
            try
            {
                if (id != client.Id)
                {
                    TempData["Error"] = "Invalid client request.";
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                    return View(client);

                await _clientRepository
                    .UpdateAsync(client, cancellationToken);

                TempData["Success"] =
                    "Client updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Client not found for update. Id: {ClientId}",
                    id);

                TempData["Error"] = "Client not found.";

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while updating client. Id: {ClientId}",
                    id);

                TempData["Error"] =
                    "An error occurred while updating the client.";

                return View(client);
            }
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository
                    .GetByIdAsync(id, cancellationToken);

                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
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

                TempData["Error"] =
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
            try
            {
                var deleted = await _clientRepository
                    .DeleteAsync(id, cancellationToken);

                if (!deleted)
                {
                    TempData["Error"] = "Client not found.";
                    return NotFound();
                }

                TempData["Success"] =
                    "Client deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while deleting client. Id: {ClientId}",
                    id);

                TempData["Error"] =
                    "An error occurred while deleting the client.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}