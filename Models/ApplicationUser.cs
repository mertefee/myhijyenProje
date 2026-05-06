using Microsoft.AspNetCore.Identity;

namespace myhijyenProje.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsCorporateApproved { get; set; } = false;

        public string? CorporateEmailDomain { get; set; }

        public DateTime? CorporateApprovedAt { get; set; }

        public string? CorporateApprovedBy { get; set; }
    }
}