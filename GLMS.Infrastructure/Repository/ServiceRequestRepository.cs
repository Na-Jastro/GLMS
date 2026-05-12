using GLMS.Core.Models;
using GLMS.Core.Repositories;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Infrastructure.Repository
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly GLMSDbContext _context;

        public ServiceRequestRepository(GLMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        public async Task<ServiceRequest?> GetDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        public async Task<ServiceRequest> CreateAsync(
            ServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            await _context.ServiceRequests
                .AddAsync(request, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return request;
        }

        public async Task<IEnumerable<Contract>> GetContractsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Contract?> GetContractAsync(
            int contractId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(
                    c => c.Id == contractId,
                    cancellationToken);
        }
    }
}