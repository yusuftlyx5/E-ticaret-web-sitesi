using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;

namespace EticaretApp.ViewComponents;

// Sepet öğe sayısını gösteren ViewComponent
public class CartCountViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartCountViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(HttpContext.User);
        int cartItemCount = 0;

        if (userId != null)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                cartItemCount = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0;
            }
        }

        return View(cartItemCount);
    }
}

