using GLMS.Core.Models;

namespace GLMS.Core.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceRequest?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceRequest> CreateAsync(
            ServiceRequest request,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Contract>> GetContractsAsync(
            CancellationToken cancellationToken = default);

        Task<Contract?> GetContractAsync(
            int contractId,
            CancellationToken cancellationToken = default);
    }
}