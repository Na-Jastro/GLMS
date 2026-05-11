using GLMS.Web.Data;
using GLMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services
{
    public class ContractService
    {
        private readonly ApplicationDbContext _context;

        public ContractService(ApplicationDbContext context)
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