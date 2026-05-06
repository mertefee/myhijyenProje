using Microsoft.AspNetCore.Identity;
using myhijyenProje.Models;
using myhijyenProje.Services;

namespace myhijyenProje.Services
{
    public class RegisterService
    {
        private readonly CorporateDiscountService _corporateDiscountService;

        public RegisterService(CorporateDiscountService corporateDiscountService)
        {
            _corporateDiscountService = corporateDiscountService;
        }

        public async Task ApplyCorporateRulesAsync(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.Email))
                return;

            var matchedRule = await _corporateDiscountService
                .GetMatchingRuleByEmailAsync(user.Email);

            if (matchedRule != null)
            {
                user.CorporateEmailDomain = matchedRule.EmailDomain;
                user.IsCorporateApproved = !matchedRule.RequiresAdminApproval;
            }
            else
            {
                user.IsCorporateApproved = true;
            }
        }
    }
}