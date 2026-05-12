using GLMS.Core.Models;
using GLMS.Infrastructure.Services;
using GLMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services
{
    public class ContractService : IContractService
    {
        private readonly GLMSDbContext _context;

        public ContractService(GLMSDbContext context)
        {
            _context = context;
        }
        public async Task AutoUpdateExpiryAsync()
        {
            var contracts = await _context.Contracts.ToListAsync();

            foreach (var contract in contracts)
            {
                if (contract.EndDate < DateTime.Today &&
                    contract.Status != ContractStatus.Expired)
                {
                    contract.Status = ContractStatus.Expired;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}