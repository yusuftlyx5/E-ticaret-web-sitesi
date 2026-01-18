using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;
using EticaretApp.Models;

namespace EticaretApp.Controllers;

// Ürün detay sayfası controller'ı
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Ürün detay sayfası
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Ürünü kategorisi ve yorumlarıyla birlikte getir
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // İlgili ürünleri getir (aynı kategoriden)
        var relatedProducts = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
            .Take(4)
            .ToListAsync();

        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }
}

