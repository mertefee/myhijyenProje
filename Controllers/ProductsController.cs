using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using myhijyenProje.Models;

namespace myhijyenProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = _context.Products.Include(p => p.Category);
            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile>? imageFiles)
        {
            if (ModelState.IsValid)
            {
                if (imageFiles != null && imageFiles.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var maxFileSize = 5 * 1024 * 1024;

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                            if (!allowedExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("", "Sadece JPG, JPEG, PNG veya WEBP yükleyebilirsin.");
                                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                                return View(product);
                            }

                            if (file.Length > maxFileSize)
                            {
                                ModelState.AddModelError("", "Bir görsel en fazla 5 MB olabilir.");
                                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                                return View(product);
                            }

                            var fileName = Guid.NewGuid() + extension;
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            product.Images.Add(new ProductImage
                            {
                                ImageUrl = "/images/products/" + fileName
                            });
                        }
                    }

                    product.ImageUrl = product.Images.First().ImageUrl;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile>? imageFiles)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                product.Images = await _context.ProductImages
                    .Where(x => x.ProductId == product.Id)
                    .ToListAsync();

                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Details = product.Details;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.CategoryId = product.CategoryId;

            if (imageFiles != null && imageFiles.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var maxFileSize = 5 * 1024 * 1024;

                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("", "Sadece JPG, JPEG, PNG veya WEBP yükleyebilirsin.");
                            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                            return View(existingProduct);
                        }

                        if (file.Length > maxFileSize)
                        {
                            ModelState.AddModelError("", "Bir görsel en fazla 5 MB olabilir.");
                            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                            return View(existingProduct);
                        }

                        var fileName = Guid.NewGuid() + extension;
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingProduct.Images.Add(new ProductImage
                        {
                            ImageUrl = "/images/products/" + fileName,
                            ProductId = existingProduct.Id
                        });
                    }
                }

                if (string.IsNullOrEmpty(existingProduct.ImageUrl) && existingProduct.Images.Any())
                {
                    existingProduct.ImageUrl = existingProduct.Images.First().ImageUrl;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);

            if (image == null)
                return NotFound();

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                image.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                if (product.ImageUrl == image.ImageUrl)
                {
                    product.ImageUrl = product.Images.FirstOrDefault()?.ImageUrl;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int imageId, int productId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);

            if (image == null)
                return NotFound();

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

            product.ImageUrl = image.ImageUrl;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                foreach (var image in product.Images)
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        image.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}