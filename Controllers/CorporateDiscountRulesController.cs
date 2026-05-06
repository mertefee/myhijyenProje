using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using myhijyenProje.Models;
using myhijyenProje.Services;

namespace myhijyenProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CorporateDiscountRulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CorporateDiscountService _discountService;

        public CorporateDiscountRulesController(
            ApplicationDbContext context,
            CorporateDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.CorporateDiscountRules
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(rules);
        }

        public IActionResult Create()
        {
            return View(new CorporateDiscountRule
            {
                IsActive = true,
                RequiresAdminApproval = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CorporateDiscountRule rule)
        {
            rule.EmailDomain = _discountService.NormalizeDomain(rule.EmailDomain);

            var exists = await _context.CorporateDiscountRules
                .AnyAsync(x => x.EmailDomain == rule.EmailDomain);

            if (exists)
            {
                ModelState.AddModelError("EmailDomain", "Bu mail uzantısı zaten eklenmiş.");
            }

            if (!ModelState.IsValid)
                return View(rule);

            rule.CreatedAt = DateTime.Now;

            _context.CorporateDiscountRules.Add(rule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rule = await _context.CorporateDiscountRules.FindAsync(id);

            if (rule == null)
                return NotFound();

            return View(rule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CorporateDiscountRule rule)
        {
            if (id != rule.Id)
                return NotFound();

            rule.EmailDomain = _discountService.NormalizeDomain(rule.EmailDomain);

            var exists = await _context.CorporateDiscountRules
                .AnyAsync(x => x.Id != rule.Id && x.EmailDomain == rule.EmailDomain);

            if (exists)
            {
                ModelState.AddModelError("EmailDomain", "Bu mail uzantısı başka bir kayıtta kullanılıyor.");
            }

            if (!ModelState.IsValid)
                return View(rule);

            _context.Update(rule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rule = await _context.CorporateDiscountRules.FindAsync(id);

            if (rule != null)
            {
                _context.CorporateDiscountRules.Remove(rule);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}