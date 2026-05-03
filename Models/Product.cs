#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myhijyenProje.Models
{
    public class Product
    {

        public List<ProductImage> Images { get; set; } = new();
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Details { get; set; }


        [Required]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçmelisiniz")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

    }
}
