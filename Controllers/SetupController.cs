using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;

namespace EticaretApp.Controllers;

// Admin kullanıcısını oluşturmak için setup controller'ı
// Bu controller'a herkes erişebilir (sadece ilk kurulum için)
public class SetupController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SetupController> _logger;

    public SetupController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ILogger<SetupController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    // Admin kullanıcısını oluştur
    [HttpGet]
    public async Task<IActionResult> CreateAdmin()
    {
        try
        {
            // Rolleri oluştur
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Admin kullanıcısını kontrol et
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Admin kullanıcısı yoksa oluştur
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    ViewBag.Message = $"Admin kullanıcısı başarıyla oluşturuldu!<br/>Email: {adminEmail}<br/>Şifre: {adminPassword}";
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Message = "Admin kullanıcısı oluşturulamadı: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    ViewBag.Success = false;
                }
            }
            else
            {
                // Admin kullanıcısı varsa şifresini sıfırla
                var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await _userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                
                if (resetResult.Succeeded)
                {
                    adminUser.EmailConfirmed = true;
                    adminUser.LockoutEnabled = false;
                    await _userManager.UpdateAsync(adminUser);
                    
                    if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    
                    ViewBag.Message = $"Admin kullanıcısının şifresi sıfırlandı!<br/>Email: {adminEmail}<br/>Şifre: {adminPassword}";
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Message = "Şifre sıfırlanamadı: " + string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    ViewBag.Success = false;
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = "Hata: " + ex.Message;
            ViewBag.Success = false;
            _logger.LogError(ex, "Admin kullanıcısı oluşturulurken hata oluştu");
        }

        return View();
    }
}















































