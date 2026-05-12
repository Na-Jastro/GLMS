using GLMS.Infrastructure.Services;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GLMS.Web.Services
{

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRate()
        {
            var response = await _httpClient.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var rate = doc.RootElement
                .GetProperty("rates")
                .GetProperty("ZAR")
                .GetDecimal();

            return rate;
        }
    }
}
