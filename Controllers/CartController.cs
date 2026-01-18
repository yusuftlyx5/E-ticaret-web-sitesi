using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;
using EticaretApp.Models;

namespace EticaretApp.Controllers;

// Sepet işlemleri controller'ı
[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Sepeti görüntüle
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Kullanıcının sepetini getir
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Sepet yoksa oluştur
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return View(cart);
    }

    // Sepete ürün ekle
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        // Ürünü kontrol et
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return Json(new { success = false, message = "Ürün bulunamadı." });
        }

        // Beden kontrolü
        if (!string.IsNullOrEmpty(product.Size) && string.IsNullOrEmpty(request.Size))
        {
            return Json(new { success = false, message = "Lütfen bir beden seçiniz." });
        }

        // Stok kontrolü
        if (product.Stock < request.Quantity)
        {
            return Json(new { success = false, message = "Yeterli stok bulunmamaktadır." });
        }

        // Kullanıcının sepetini getir veya oluştur
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Sepette aynı ürün VE aynı beden var mı kontrol et
        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId && ci.Size == request.Size);
        if (existingItem != null)
        {
            // Varsa miktarı artır
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            // Yoksa yeni öğe ekle
            cart.CartItems.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Size = request.Size // Beden bilgisini kaydet
            });
        }

        cart.UpdatedDate = DateTime.Now;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Ürün sepete eklendi." });
    }

    // Sepetten ürün sil
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.Cart.UserId == userId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Sepet öğesi bulunamadı." });
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Ürün sepetten kaldırıldı." });
    }

    // Sepet miktarını güncelle
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
        }

        if (request.Quantity < 1)
        {
            return Json(new { success = false, message = "Miktar en az 1 olmalıdır." });
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.Cart.UserId == userId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Sepet öğesi bulunamadı." });
        }

        // Stok kontrolü
        if (cartItem.Product.Stock < request.Quantity)
        {
            return Json(new { success = false, message = "Yeterli stok bulunmamaktadır." });
        }

        cartItem.Quantity = request.Quantity;
        cartItem.Cart.UpdatedDate = DateTime.Now;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Miktar güncellendi." });
    }

    // Sepet öğe sayısını getir (AJAX için)
    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var userId = _userManager.GetUserId(User);
        int count = 0;

        if (userId != null)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems != null)
            {
                count = cart.CartItems.Sum(ci => ci.Quantity);
            }
        }

        return Json(new { count = count });
    }
}

// Request modelleri
public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Size { get; set; }
}

public class RemoveFromCartRequest
{
    public int CartItemId { get; set; }
}

public class UpdateQuantityRequest
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
}

