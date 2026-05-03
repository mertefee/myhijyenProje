using System.ComponentModel.DataAnnotations;

namespace myhijyenProje.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string? UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required, MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}