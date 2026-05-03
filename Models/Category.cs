using System.ComponentModel.DataAnnotations;

namespace myhijyenProje.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        public string Name { get; set; }

        // Navigation property validation patlatmasın:
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
