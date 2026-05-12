using GLMS.Core.Models;

namespace GLMS.Core.Repositories
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync(
            DateTime? start = null,
            DateTime? end = null,
            ContractStatus? status = null,
            int? clientId = null,
            CancellationToken cancellationToken = default);

        Task<Contract?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Contract?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Contract> CreateAsync(
            Contract contract,
            CancellationToken cancellationToken = default);

        Task<Contract> UpdateAsync(
            Contract contract,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> GetTotalCountAsync(
            CancellationToken cancellationToken = default);

        Task<int> GetActiveCountAsync(
            CancellationToken cancellationToken = default);

        Task<int> GetExpiredCountAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Client>> GetClientsAsync(
            CancellationToken cancellationToken = default);
    }
}
