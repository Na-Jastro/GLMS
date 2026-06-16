using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Client?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Client?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> CreateAsync(
            Client client,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Client client,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}