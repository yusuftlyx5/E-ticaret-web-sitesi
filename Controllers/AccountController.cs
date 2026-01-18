using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EticaretApp.Models;

namespace EticaretApp.Controllers;

// Hesap işlemleri controller'ı (Login, Register, Logout)
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // Kayıt sayfası
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Kayıt işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // User rolünün var olduğundan emin ol
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Kullanıcıya User rolünü ata
                await _userManager.AddToRoleAsync(user, "User");

                // Otomatik giriş yap
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // Giriş sayfası
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Giriş işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // Önce kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Kullanıcı yoksa UserName ile de dene
                user = await _userManager.FindByNameAsync(model.Email);
            }

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName ?? model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Hesabınız kilitlenmiştir.");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Giriş yapmanıza izin verilmiyor.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz şifre.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta adresi ile kayıtlı kullanıcı bulunamadı.");
            }
        }

        return View(model);
    }

    // Çıkış işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // Erişim reddedildi sayfası
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

