using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Infrastructure
{
    public class ClientRepository : IClientRepository
    {
        private readonly GLMSDbContext _context;

        public ClientRepository(GLMSDbContext context)
        {
            _context = context;
        }

        #region Queries

        public async Task<IEnumerable<Client>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Client?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);
        }

        public async Task<Client?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .Include(c => c.Contracts)
                .ThenInclude(c => c.ServiceRequests)
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AnyAsync(
                    c => c.Id == id,
                    cancellationToken);
        }

        #endregion

        #region Commands

        public async Task<Client> CreateAsync(
            Client client,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);

            await _context.Clients
                .AddAsync(client, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return client;
        }

        public async Task<Client> UpdateAsync(
            Client client,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);

            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(
                    c => c.Id == client.Id,
                    cancellationToken);

            if (existingClient == null)
                throw new KeyNotFoundException(
                    $"Client with Id {client.Id} was not found.");

            existingClient.Name = client.Name;
            existingClient.ContactDetails = client.ContactDetails;
            existingClient.Region = client.Region;

            await _context.SaveChangesAsync(cancellationToken);

            return existingClient;
        }

        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);

            if (client == null)
                return false;

            _context.Clients.Remove(client);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        #endregion
    }
}