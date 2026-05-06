using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using myhijyenProje.Models;
using myhijyenProje.Services;

namespace myhijyenProje.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CorporateDiscountService _discountService;

        public StoreController(
            ApplicationDbContext context,
            CorporateDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? q)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(q) ||
                    (p.Description != null && p.Description.Contains(q)) ||
                    (p.Details != null && p.Details.Contains(q)));
            }

            var products = await productsQuery.ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryChipVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = _context.Products.Count(p => p.CategoryId == c.Id)
                })
                .ToListAsync();

            var discountRate = await _discountService.GetUserDiscountRateAsync(User);

            ViewBag.Categories = categories;
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.SearchQuery = q ?? "";
            ViewBag.DiscountRate = discountRate;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var similarProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(8)
                .ToListAsync();

            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var discountRate = await _discountService.GetUserDiscountRateAsync(User);
            var discountedPrice = await _discountService.ApplyDiscountAsync(product.Price, User);

            ViewBag.SimilarProducts = similarProducts;
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.DiscountRate = discountRate;
            ViewBag.DiscountedPrice = discountedPrice;

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var discountRate = await _discountService.GetUserDiscountRateAsync(User);
            var discountedPrice = await _discountService.ApplyDiscountAsync(product.Price, User);

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                originalPrice = product.Price,
                price = discountedPrice,
                discountRate = discountRate,
                hasDiscount = discountRate > 0,
                imageUrl = product.ImageUrl,
                description = product.Description,
                details = product.Details,
                categoryName = product.Category != null ? product.Category.Name : null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
            {
                return RedirectToAction("Details", new { id = productId });
            }

            var review = new ProductReview
            {
                ProductId = productId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ProductReviews.Add(review);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = productId });
        }
    }
}