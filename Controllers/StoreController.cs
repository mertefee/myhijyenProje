using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using myhijyenProje.Models;

namespace myhijyenProje.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StoreController(ApplicationDbContext context) => _context = context;

        // /Store?categoryId=2&q=xxx
        public IActionResult Index(int? categoryId, string? q)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(q) ||
                    (p.Description != null && p.Description.Contains(q)) ||
                    (p.Details != null && p.Details.Contains(q))
                );
            }

            var products = productsQuery.ToList();

            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryChipVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = _context.Products.Count(p => p.CategoryId == c.Id)
                })
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.SearchQuery = q ?? "";

            return View(products);
        }

        // /Store/Details/5
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            var similar = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(8)
                .ToList();

            var reviews = _context.ProductReviews
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ViewBag.SimilarProducts = similar;
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return View(product);
        }

        // QuickView JSON
        [HttpGet]
        public IActionResult QuickView(int id)
        {
            var p = _context.Products
                .Include(x => x.Category)
                .FirstOrDefault(x => x.Id == id);

            if (p == null) return NotFound();

            return Json(new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                imageUrl = p.ImageUrl,
                description = p.Description,
                details = p.Details,
                categoryName = p.Category != null ? p.Category.Name : null
            });
        }

        // Yorum ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
                return RedirectToAction("Details", new { id = productId });

            _context.ProductReviews.Add(new ProductReview
            {
                ProductId = productId,
                Rating = rating,
                Comment = comment.Trim()
            });

            _context.SaveChanges();
            return RedirectToAction("Details", new { id = productId });
        }
    }
}