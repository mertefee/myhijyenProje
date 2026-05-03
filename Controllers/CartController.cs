using Microsoft.AspNetCore.Mvc;
using myhijyenProje.Data;
using myhijyenProje.Models;
using Newtonsoft.Json;

namespace myhijyenProje.Controllers
{
    public class CartController : Controller
    {

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context) => _context = context;

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
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductId == id);

            if (existing != null) existing.Quantity++;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });

            SaveCart(cart);

            var cartCount = cart.Sum(x => x.Quantity);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    cartCount,
                    message = $"{product.Name} sepete eklendi ✅"
                });
            }

            var referer = Request.Headers["Referer"].ToString();
            return string.IsNullOrWhiteSpace(referer)
                ? RedirectToAction("Index", "Store")
                : Redirect(referer);
        }

        public IActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) item.Quantity++;
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
                if (item.Quantity <= 0) cart.Remove(item);
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }
    }



}