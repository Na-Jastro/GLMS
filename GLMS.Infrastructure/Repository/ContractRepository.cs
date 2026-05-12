using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Infrastructure.Repository
{
    public class ContractRepository : IContractRepository
    {
        private readonly GLMSDbContext _context;

        public ContractRepository(GLMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(
            DateTime? start = null,
            DateTime? end = null,
            ContractStatus? status = null,
            int? clientId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsNoTracking()
                .AsQueryable();

            if (start.HasValue)
                query = query.Where(c => c.StartDate >= start.Value);

            if (end.HasValue)
                query = query.Where(c => c.EndDate <= end.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (clientId.HasValue)
                query = query.Where(c => c.ClientId == clientId.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Contract?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);
        }

        public async Task<Contract?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);
        }

        public async Task<Contract> CreateAsync(
            Contract contract,
            CancellationToken cancellationToken = default)
        {
            await _context.Contracts
                .AddAsync(contract, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine("error", ex.ToString());
            }

            return contract;
        }

        public async Task<Contract> UpdateAsync(
            Contract contract,
            CancellationToken cancellationToken = default)
        {
            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(
                    c => c.Id == contract.Id,
                    cancellationToken);

            if (existingContract == null)
                throw new KeyNotFoundException(
                    $"Contract with Id {contract.Id} was not found.");

            existingContract.ClientId = contract.ClientId;
            existingContract.StartDate = contract.StartDate;
            existingContract.EndDate = contract.EndDate;
            existingContract.Status = contract.Status;
            existingContract.ServiceLevel = contract.ServiceLevel;
            existingContract.SignedAgreementPath = contract.SignedAgreementPath;

            await _context.SaveChangesAsync(cancellationToken);

            return existingContract;
        }

        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    cancellationToken);

            if (contract == null)
                return false;

            _context.Contracts.Remove(contract);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<int> GetTotalCountAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetActiveCountAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .CountAsync(
                    c => c.Status == ContractStatus.Active,
                    cancellationToken);
        }

        public async Task<int> GetExpiredCountAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .CountAsync(
                    c => c.Status == ContractStatus.Expired,
                    cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetClientsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
    }
}