using System.ComponentModel.DataAnnotations;

namespace ProxiCall.CRM.Models
{
    public class Product
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
