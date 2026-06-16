using System.Net.Http.Headers;
using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public class ContractApiService : IContractApiService
    {
        private readonly HttpClient _httpClient;

        public ContractApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Contract>> GetAllAsync(
            DateTime? start = null,
            DateTime? end = null,
            ContractStatus? status = null,
            int? clientId = null,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>();

            if (start.HasValue)
                query.Add($"start={start.Value:yyyy-MM-dd}");

            if (end.HasValue)
                query.Add($"end={end.Value:yyyy-MM-dd}");

            if (status.HasValue)
                query.Add($"status={(int)status.Value}");

            if (clientId.HasValue)
                query.Add($"clientId={clientId}");

            var url = "api/ContractsApi";

            if (query.Any())
                url += "?" + string.Join("&", query);

            return await _httpClient.GetFromJsonAsync<List<Contract>>(
                       url,
                       cancellationToken)
                   ?? new List<Contract>();
        }

        public async Task<Contract?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<Contract>(
                $"api/ContractsApi/{id}",
                cancellationToken);
        }

        public async Task<bool> CreateAsync(
            Contract contract,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/ContractsApi",
                    contract,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(
            Contract contract,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _httpClient.PutAsJsonAsync(
                    $"api/ContractsApi/{contract.Id}",
                    contract,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"api/ContractsApi/{id}",
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<Client>> GetClientsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<List<Client>>(
                       "api/ContractsApi/clients",
                       cancellationToken)
                   ?? new List<Client>();
        }

        public async Task<ContractStatisticsDto?> GetStatisticsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<ContractStatisticsDto>(
                "api/ContractsApi/statistics",
                cancellationToken);
        }

        public async Task<bool> UploadAgreementAsync(
            int id,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            using var form = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/pdf");

            form.Add(
                fileContent,
                "file",
                file.FileName);

            var response =
                await _httpClient.PostAsync(
                    $"api/ContractsApi/{id}/upload-agreement",
                    form,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]?> DownloadAgreementAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _httpClient.GetAsync(
                    $"api/ContractsApi/{id}/download-agreement",
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadAsByteArrayAsync(cancellationToken);
        }
    }
}