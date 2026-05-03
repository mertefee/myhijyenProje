using System.ComponentModel.DataAnnotations;

namespace myhijyenProje.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required] public string FullName { get; set; }
        [Required] public string Phone { get; set; }
        [Required] public string Address { get; set; }
        public string? Note { get; set; }

        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Hazırlanıyor";

        public List<OrderItem> Items { get; set; } = new();
    }
}