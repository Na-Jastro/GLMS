using System.Net.Http.Json;
using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientService> _logger;

        private const string BaseUrl = "api/ClientsApi";

        public ClientService(
            HttpClient httpClient,
            ILogger<ClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Client>>(
                           BaseUrl,
                           cancellationToken)
                       ?? new List<Client>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving clients");

                return new List<Client>();
            }
        }

        public async Task<Client?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Client>(
                    $"{BaseUrl}/{id}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving client {ClientId}",
                    id);

                return null;
            }
        }

        public async Task<Client?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(
                id,
                cancellationToken);
        }

        public async Task<bool> CreateAsync(
            Client client,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    BaseUrl,
                    client,
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating client");

                return false;
            }
        }

        public async Task<bool> UpdateAsync(
            Client client,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/{client.Id}",
                    client,
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating client {ClientId}",
                    client.Id);

                return false;
            }
        }

        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{BaseUrl}/{id}",
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting client {ClientId}",
                    id);

                return false;
            }
        }
    }
}