using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace myhijyenProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        public IActionResult DownloadPdf(int id)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("Sipariş Detayı")
                        .FontSize(22)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text($"Sipariş No: {order.Id}");
                        col.Item().Text($"Müşteri: {order.FullName}");
                        col.Item().Text($"Telefon: {order.Phone}");
                        col.Item().Text($"Adres: {order.Address}");
                        col.Item().Text($"Durum: {order.Status}");
                        col.Item().Text($"Tarih: {order.CreatedAt:dd.MM.yyyy HH:mm}");

                        col.Item().PaddingTop(15).Text("Ürünler").FontSize(16).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Ürün").Bold();
                                header.Cell().Text("Adet").Bold();
                                header.Cell().Text("Fiyat").Bold();
                                header.Cell().Text("Toplam").Bold();
                            });

                            foreach (var item in order.Items)
                            {
                                table.Cell().Text(item.ProductName);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.UnitPrice} TL");
                                table.Cell().Text($"{item.UnitPrice * item.Quantity} TL");
                            }
                        });

                        col.Item().PaddingTop(15)
                            .AlignRight()
                            .Text($"Genel Toplam: {order.Total} TL")
                            .FontSize(16)
                            .Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("myhijyenProje - Sipariş Çıktısı");
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"siparis_{order.Id}.pdf");
        }
    }
}