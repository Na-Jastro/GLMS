using System.ComponentModel.DataAnnotations;

namespace GLMS.Core.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? ContactDetails { get; set; }
        public string? Region { get; set; }

        public ICollection<Contract>? Contracts { get; set; }
    }
}