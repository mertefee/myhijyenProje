using Microsoft.AspNetCore.Mvc;
using myhijyenProje.Data;
using myhijyenProje.Models;
using myhijyenProje.Services;
using Newtonsoft.Json;

namespace myhijyenProje.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CorporateDiscountService _discountService;

        public CartController(
            ApplicationDbContext context,
            CorporateDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product != null)
                {
                    item.Price = await _discountService.ApplyDiscountAsync(product.Price, User);
                    item.ProductName = product.Name;
                }
            }

            SaveCart(cart);

            return View(cart);
        }

        [HttpGet]
        public IActionResult GetCartSummary()
        {
            var cart = GetCart();

            var items = cart.Select(x => new
            {
                productId = x.ProductId,
                productName = x.ProductName,
                price = x.Price,
                quantity = x.Quantity,
                total = x.Price * x.Quantity
            }).ToList();

            return Json(new
            {
                success = true,
                items = items,
                cartCount = cart.Sum(x => x.Quantity),
                grandTotal = cart.Sum(x => x.Price * x.Quantity)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            if (product.Stock <= 0)
            {
                return Json(new { success = false, message = "Bu ürün stokta yok." });
            }

            var cart = GetCart();

            var existing = cart.FirstOrDefault(x => x.ProductId == id);

            var finalPrice = await _discountService.ApplyDiscountAsync(product.Price, User);

            if (existing != null)
            {
                if (existing.Quantity >= product.Stock)
                {
                    return Json(new { success = false, message = "Stoktan fazla ürün ekleyemezsin." });
                }

                existing.Quantity++;
                existing.Price = finalPrice;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = finalPrice,
                    Quantity = 1
                });
            }

            SaveCart(cart);

            var cartCount = cart.Sum(x => x.Quantity);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    cartCount,
                    message = $"{product.Name} sepete eklendi."
                });
            }

            var referer = Request.Headers["Referer"].ToString();

            return string.IsNullOrWhiteSpace(referer)
                ? RedirectToAction("Index", "Store")
                : Redirect(referer);
        }

        public async Task<IActionResult> Increase(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(c => c.ProductId == id);
            var product = await _context.Products.FindAsync(id);

            if (item != null && product != null)
            {
                if (item.Quantity < product.Stock)
                {
                    item.Quantity++;
                    item.Price = await _discountService.ApplyDiscountAsync(product.Price, User);
                }
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }
    }
}