using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using myhijyenProje.Models;
using System.Security.Claims;

namespace myhijyenProje.Services
{
    public class CorporateDiscountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CorporateDiscountService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<decimal> GetUserDiscountRateAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
                return 0;

            var user = await _userManager.GetUserAsync(principal);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return 0;

            var email = user.Email.ToLower().Trim();

            var rule = await _context.CorporateDiscountRules
                .Where(x => x.IsActive && email.EndsWith(x.EmailDomain.ToLower()))
                .OrderByDescending(x => x.DiscountPercentage)
                .FirstOrDefaultAsync();

            if (rule == null)
                return 0;

            if (rule.RequiresAdminApproval && !user.IsCorporateApproved)
                return 0;

            return rule.DiscountPercentage;
        }

        public async Task<decimal> ApplyDiscountAsync(decimal originalPrice, ClaimsPrincipal principal)
        {
            var rate = await GetUserDiscountRateAsync(principal);

            if (rate <= 0)
                return originalPrice;

            return Math.Round(originalPrice - (originalPrice * rate / 100), 2);
        }

        public async Task<CorporateDiscountRule?> GetMatchingRuleByEmailAsync(string email)
        {
            email = email.ToLower().Trim();

            return await _context.CorporateDiscountRules
                .Where(x => x.IsActive && email.EndsWith(x.EmailDomain.ToLower()))
                .OrderByDescending(x => x.DiscountPercentage)
                .FirstOrDefaultAsync();
        }

        public string NormalizeDomain(string domain)
        {
            domain = domain.ToLower().Trim();

            if (!domain.StartsWith("@"))
                domain = "@" + domain;

            return domain;
        }
    }
}