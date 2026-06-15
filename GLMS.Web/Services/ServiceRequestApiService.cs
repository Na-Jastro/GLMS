using System.Net.Http.Json;
using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public class ServiceRequestApiService
        : IServiceRequestApiService
    {
        private readonly HttpClient _httpClient;

        public ServiceRequestApiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ServiceRequest>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _httpClient
                       .GetFromJsonAsync<List<ServiceRequest>>(
                           "api/ServiceRequestsApi",
                           cancellationToken)
                   ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _httpClient
                .GetFromJsonAsync<ServiceRequest>(
                    $"api/ServiceRequestsApi/{id}",
                    cancellationToken);
        }

        public async Task<bool> CreateAsync(
            ServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/ServiceRequestsApi",
                    request,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<ContractLookupDto>>
            GetContractsAsync(
                CancellationToken cancellationToken = default)
        {
            return await _httpClient
                       .GetFromJsonAsync<
                           List<ContractLookupDto>>(
                           "api/ServiceRequestsApi/contracts",
                           cancellationToken)
                   ?? new List<ContractLookupDto>();
        }

        public async Task<CurrencyConversionDto?>
            ConvertUsdToZarAsync(
                decimal usd,
                CancellationToken cancellationToken = default)
        {
            return await _httpClient
                .GetFromJsonAsync<CurrencyConversionDto>(
                    $"api/ServiceRequestsApi/convert-usd-to-zar?usd={usd}",
                    cancellationToken);
        }
    }
}