using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using myhijyenProje.Data;
using myhijyenProje.Models;
using Newtonsoft.Json;

namespace myhijyenProje.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = GetCart();

            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string fullName, string phone, string address, string? note)
        {
            var cart = GetCart();

            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(address))
            {
                ModelState.AddModelError("", "Ad Soyad, Telefon ve Adres zorunludur.");
                return View("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user?.Id,
                FullName = fullName.Trim(),
                Phone = phone.Trim(),
                Address = address.Trim(),
                Note = note?.Trim()
            };

            foreach (var c in cart)
            {
                var product = _context.Products.Find(c.ProductId);

                if (product == null)
                    return RedirectToAction("Index", "Cart");

                if (product.Stock < c.Quantity)
                {
                    ModelState.AddModelError("", $"{product.Name} için yeterli stok yok.");
                    return View("Index");
                }

                product.Stock -= c.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    UnitPrice = c.Price,
                    Quantity = c.Quantity
                });
            }

            order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
        }
    }
}