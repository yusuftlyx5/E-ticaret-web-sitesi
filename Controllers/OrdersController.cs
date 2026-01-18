using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;
using EticaretApp.Models;

namespace EticaretApp.Controllers;

// Sipariş işlemleri controller'ı
[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Sipariş listesi
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Kullanıcının siparişlerini getir
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // Sipariş oluştur sayfası
    [HttpGet]
    public async Task<IActionResult> Create()
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

        if (cart == null || !cart.CartItems.Any())
        {
            TempData["ErrorMessage"] = "Sepetiniz boş.";
            return RedirectToAction("Index", "Cart");
        }

        return View();
    }

    // Sipariş oluştur - Adres Adımı
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Order order)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // UserId ve TotalAmount validation hatalarını kaldır
        ModelState.Remove("UserId");
        ModelState.Remove("TotalAmount");
        ModelState.Remove("OrderDate");
        ModelState.Remove("Status");

        // Kullanıcının sepetini getir - Tutar hesaplamak için
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            TempData["ErrorMessage"] = "Sepetiniz boş.";
            return RedirectToAction("Index", "Cart");
        }

        if (ModelState.IsValid)
        {
            // Stok kontrolü
            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product == null || cartItem.Product.Stock < cartItem.Quantity)
                {
                    TempData["ErrorMessage"] = $"{cartItem.Product?.Name ?? "Ürün"} için yeterli stok bulunmamaktadır.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Sipariş bilgilerini TempData'ya kaydet (JSON olarak)
            order.UserId = userId;
            order.TotalAmount = cart.TotalAmount; // Tutarı şimdilik cart'tan alıyoruz, ödeme ekranında göstereceğiz
            
            TempData["OrderData"] = System.Text.Json.JsonSerializer.Serialize(order);
            
            return RedirectToAction("Payment");
        }

        return View(order);
    }

    // Ödeme Ekranı
    [HttpGet]
    public IActionResult Payment()
    {
        if (TempData["OrderData"] == null)
        {
            return RedirectToAction("Create");
        }

        var orderJson = TempData["OrderData"]!.ToString();
        // TempData okunduğunda silinir, bir sonraki request (POST) için tekrar koyalım
        TempData.Keep("OrderData"); 

        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(orderJson);

        var model = new PaymentViewModel
        {
            Order = order
        };

        return View(model);
    }

    // Ödeme İşlemi ve Sipariş Tamamlama
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(PaymentViewModel model)
    {
        // Address, Phone vb. için Order içindeki validasyonları manuel kontrol etmeye gerek yok 
        // çünkü önceki adımda geçildi, ancak hidden field olarak geldikleri için model state check edebiliriz.
        // PaymentViewModel içindeki validasyonlar (Kart bilgileri) önemli.

        // Model.Order null gelirse veya önceki datayı kaybettiysek
        if (TempData["OrderData"] != null)
        {
             // Güvenlik için Order datasını TempData'dan da alabiliriz veya formdan gelenle kıyaslayabiliriz.
             // Kolaylık olması açısından: 
             var originalOrder = System.Text.Json.JsonSerializer.Deserialize<Order>(TempData["OrderData"]!.ToString());
             
             // Formdan gelen order (hidden fields) ile işlem yapalım ama UserId vb güvenli alanları restore edelim.
             model.Order.UserId = originalOrder.UserId;
             model.Order.TotalAmount = originalOrder.TotalAmount; // Tutar değişmemeli
        }
        else
        {
             TempData["ErrorMessage"] = "Oturum süreniz doldu, lütfen tekrar deneyin.";
             return RedirectToAction("Create");
        }
        
        // Validation - Order alanlarını ignore et (zaten doldu varsayıyoruz veya tekrar validate edebiliriz)
        // Kart bilgilerini kontrol et
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // --- BURADA ÖDEME İŞLEMİ YAPILIR (MOCK) ---
        // bool paymentSuccess = PaymentService.Pay(model.CardNumber, model.Order.TotalAmount);
        bool paymentSuccess = true; 

        if (!paymentSuccess)
        {
            ModelState.AddModelError("", "Ödeme işlemi başarısız oldu.");
            return View(model);
        }

        // --- SİPARİŞİ VERİTABANINA KAYDET ---
        var userId = _userManager.GetUserId(User); // Tekrar alalım garanti olsun
        
         // Kullanıcının sepetini tekrar getir (Stok düşmek vb için)
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
            
        if (cart == null || !cart.CartItems.Any())
        {
             TempData["ErrorMessage"] = "Sepetinizde ürün bulunamadı.";
             return RedirectToAction("Index", "Cart");
        }

        // Entity doldur
        var order = model.Order;
        order.UserId = userId;
        order.OrderDate = DateTime.Now;
        order.Status = OrderStatus.Pending; // Ödeme alındı, hazırlanıyor vs.
        order.TotalAmount = cart.TotalAmount; // Son tutar

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Sipariş öğelerini oluştur
        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.Price,
                Size = cartItem.Size
            };
            _context.OrderItems.Add(orderItem);

            // Stoktan düş
            if(cartItem.Product != null) 
                 cartItem.Product.Stock -= cartItem.Quantity;
        }

        // Sepeti temizle
        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Siparişiniz başarıyla alındı. Teşekkür ederiz!";
        return RedirectToAction("Index");
    }

    // Sipariş detayı
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}

