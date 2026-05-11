#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myhijyenProje.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(3000, ErrorMessage = "Ürün detayları en fazla 3000 karakter olabilir.")]
        public string? Details { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, 999999, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal Price { get; set; }

        [Range(0, 99999, ErrorMessage = "Stok negatif olamaz.")]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçmelisiniz.")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public List<ProductImage> Images { get; set; } = new();
    }
}