using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public interface IContractApiService
    {
        Task<List<Contract>> GetAllAsync(
            DateTime? start = null,
            DateTime? end = null,
            ContractStatus? status = null,
            int? clientId = null,
            CancellationToken cancellationToken = default);

        Task<Contract?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> CreateAsync(
            Contract contract,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Contract contract,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<List<Client>> GetClientsAsync(
            CancellationToken cancellationToken = default);

        Task<ContractStatisticsDto?> GetStatisticsAsync(
            CancellationToken cancellationToken = default);

        Task<bool> UploadAgreementAsync(
            int id,
            IFormFile file,
            CancellationToken cancellationToken = default);

        Task<byte[]?> DownloadAgreementAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}