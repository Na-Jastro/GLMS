using GLMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLMS.Core.Repositories
{

    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Client?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Client?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Client> CreateAsync(
            Client client,
            CancellationToken cancellationToken = default);

        Task<Client> UpdateAsync(
            Client client,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
