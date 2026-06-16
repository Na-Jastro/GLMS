using GLMS.Core.Models;
using GLMS.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClientsApiController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientsApiController> _logger;

        public ClientsApiController(
            IClientRepository clientRepository,
            ILogger<ClientsApiController> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        // GET: api/clients
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll(
            CancellationToken cancellationToken)
        {
            try
            {
                var clients = await _clientRepository
                    .GetAllAsync(cancellationToken);

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

        // GET: api/clients/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository
                    .GetDetailsAsync(id, cancellationToken);

                if (client == null)
                    return NotFound();

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving client {ClientId}",
                    id);

                return StatusCode(500,
                    "An error occurred while retrieving the client.");
            }
        }

        // POST: api/clients
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Client>> Create(
            [FromBody] Client client,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _clientRepository
                    .CreateAsync(client, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = client.Id },
                    client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating client.");

                return StatusCode(500,
                    "An error occurred while creating the client.");
            }
        }

        // PUT: api/clients/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] Client client,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != client.Id)
                    return BadRequest(
                        "Route Id does not match Client Id.");

                await _clientRepository
                    .UpdateAsync(client, cancellationToken);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating client {ClientId}",
                    id);

                return StatusCode(500,
                    "An error occurred while updating the client.");
            }
        }

        // DELETE: api/clients/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _clientRepository
                    .DeleteAsync(id, cancellationToken);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting client {ClientId}",
                    id);

                return StatusCode(500,
                    "An error occurred while deleting the client.");
            }
        }
    }
}
