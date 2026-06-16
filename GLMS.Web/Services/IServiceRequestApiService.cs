using GLMS.Core.Models;

namespace GLMS.Web.Services
{
    public interface IServiceRequestApiService
    {
        Task<List<ServiceRequest>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> CreateAsync(
            ServiceRequest request,
            CancellationToken cancellationToken = default);

        Task<List<ContractLookupDto>> GetContractsAsync(
            CancellationToken cancellationToken = default);

        Task<CurrencyConversionDto?> ConvertUsdToZarAsync(
            decimal usd,
            CancellationToken cancellationToken = default);
    }

    public class ContractLookupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class CurrencyConversionDto
    {
        public decimal USD { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal ZAR { get; set; }
    }
}