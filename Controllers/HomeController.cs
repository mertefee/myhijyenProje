using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myhijyenProje.Models;
using myhijyenProje.Data;
using myhijyenProje.Models;
using Microsoft.EntityFrameworkCore;


namespace myhijyenProje.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? categoryId)
        {
            var productsQuery = _context.Products.AsQueryable();

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            var products = productsQuery.ToList();
            return View(products);
        }

    }
}
