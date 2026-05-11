using GLMS.Core.Models;

namespace GLMS.Web.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Open";
        public decimal AmountUSD { get; set; }

        public decimal LocalCostZAR { get; set; }
    }
}
