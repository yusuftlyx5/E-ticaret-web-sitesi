using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Models;
using EticaretApp.Data;

namespace EticaretApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Ana sayfa - Ürün listesi
    public async Task<IActionResult> Index(int? categoryId)
    {
        // Kategorileri gönder (filtreleme için)
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.SelectedCategoryId = categoryId;

        // Kategoriye göre filtrele
        IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await productsQuery.ToListAsync();

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
