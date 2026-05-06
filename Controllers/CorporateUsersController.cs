using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Models;
using myhijyenProje.Services;

namespace myhijyenProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CorporateUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CorporateDiscountService _discountService;

        public CorporateUsersController(
            UserManager<ApplicationUser> userManager,
            CorporateDiscountService discountService)
        {
            _userManager = userManager;
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Where(x => x.CorporateEmailDomain != null)
                .OrderBy(x => x.IsCorporateApproved)
                .ThenBy(x => x.Email)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.IsCorporateApproved = true;
            user.CorporateApprovedAt = DateTime.Now;
            user.CorporateApprovedBy = User.Identity?.Name;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.IsCorporateApproved = false;
            user.CorporateApprovedAt = null;
            user.CorporateApprovedBy = null;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}