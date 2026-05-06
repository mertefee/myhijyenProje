using System.ComponentModel.DataAnnotations;

namespace myhijyenProje.Models
{
    public class CorporateDiscountRule
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mail uzantısı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Mail Uzantısı")]
        public string EmailDomain { get; set; } = string.Empty;

        [Required(ErrorMessage = "İndirim oranı zorunludur.")]
        [Range(0, 100, ErrorMessage = "İndirim oranı 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "İndirim Oranı")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Admin Onayı Gerekli mi?")]
        public bool RequiresAdminApproval { get; set; } = true;

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}