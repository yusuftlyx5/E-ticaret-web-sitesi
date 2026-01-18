using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;
using EticaretApp.Models;

namespace EticaretApp.Controllers;

// Admin panel controller'ı - Sadece Admin rolüne sahip kullanıcılar erişebilir
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Admin ana sayfa
    public IActionResult Index()
    {
        return View();
    }

    // Ürün yönetimi
    public async Task<IActionResult> Products()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
        return View(products);
    }

    // Ürün oluştur sayfası
    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    // Ürün oluştur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            // Resim yükleme işlemi
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Benzersiz dosya adı oluştur
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // ImageUrl'i ayarla
                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }

            product.CreatedDate = DateTime.Now;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ürün başarıyla eklendi.";
            return RedirectToAction("Products");
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(product);
    }

    // Ürün düzenle sayfası
    [HttpGet]
    public async Task<IActionResult> EditProduct(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(product);
    }

    // Ürün düzenle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Yeni resim yüklendiyse
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    
                    // Klasör yoksa oluştur
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Eski resmi sil (eğer varsa)
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Benzersiz dosya adı oluştur
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Dosyayı kaydet
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // ImageUrl'i ayarla
                    product.ImageUrl = $"/images/products/{uniqueFileName}";
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction("Products");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(product);
    }

    // Ürün detayı
    [HttpGet]
    public async Task<IActionResult> ProductDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // Ürün sil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("Products");
            }

            // İlişkili sipariş öğeleri var mı kontrol et
            var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (hasOrderItems)
            {
                TempData["ErrorMessage"] = "Bu ürün siparişlerde kullanıldığı için silinemez.";
                return RedirectToAction("Products");
            }

            // İlişkili sepet öğelerini sil
            var cartItems = await _context.CartItems.Where(ci => ci.ProductId == id).ToListAsync();
            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            // Ürün resmini sil
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Ürün resmi silinirken hata oluştu: {ImagePath}", imagePath);
                    }
                }
            }

            // Yorumları sil (zaten cascade delete ile silinir ama manuel de silebiliriz)
            _context.Reviews.RemoveRange(product.Reviews);

            // Ürünü sil
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün silinirken hata oluştu: ProductId={ProductId}", id);
            TempData["ErrorMessage"] = "Ürün silinirken bir hata oluştu: " + ex.Message;
        }

        return RedirectToAction("Products");
    }

    // Kategori yönetimi
    public async Task<IActionResult> Categories()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    // Kategori oluştur sayfası
    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }

    // Kategori oluştur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
            return RedirectToAction("Categories");
        }

        return View(category);
    }

    // Kategori düzenle sayfası
    [HttpGet]
    public async Task<IActionResult> EditCategory(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // Kategori düzenle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction("Categories");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }
        return View(category);
    }

    // Kategori sil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "Kategori bulunamadı.";
            return RedirectToAction("Categories");
        }

        // Kategoriye ait ürün var mı kontrol et
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["ErrorMessage"] = "Bu kategoriye ait ürünler olduğu için silinemez.";
            return RedirectToAction("Categories");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
        
        return RedirectToAction("Categories");
    }

    // Sipariş yönetimi
    public async Task<IActionResult> Orders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // Kullanıcı adlarını al
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var userNames = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email);

        ViewBag.UserNames = userNames;
        
        return View(orders);
    }

    // Sipariş detayı (Admin için)
    [HttpGet]
    public async Task<IActionResult> OrderDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // Sipariş durumunu güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sipariş durumu güncellendi.";
        }

        return RedirectToAction("Orders");
    }

    // Ürün var mı kontrol et
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}

