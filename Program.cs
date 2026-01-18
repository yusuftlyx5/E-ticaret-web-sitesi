using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EticaretApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection String'i al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext'i ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity yapılandırması
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Şifre ayarları
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Kullanıcı ayarları
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Veritabanını oluştur ve seed data ekle
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Veritabanı migration'ları uygula (eğer yapılmadıysa)
        // context.Database.EnsureCreated(); // Migration kullanıldığı için bu satırı kaldırdık

        // Seed Data - Kategoriler ve Ürünler
        if (!context.Categories.Any())
        {
            var category1 = new EticaretApp.Models.Category { Name = "Elektronik", Description = "Elektronik ürünler" };
            var category2 = new EticaretApp.Models.Category { Name = "Giyim", Description = "Giyim ve aksesuar" };
            var category3 = new EticaretApp.Models.Category { Name = "Ev & Yaşam", Description = "Ev ve yaşam ürünleri" };
            
            context.Categories.Add(category1);
            context.Categories.Add(category2);
            context.Categories.Add(category3);
            await context.SaveChangesAsync();
            
            // Kategoriler kaydedildikten sonra ID'lerini al
            var elektronikId = category1.Id;
            var giyimId = category2.Id;
            var evYasamId = category3.Id;

            // Ürünleri ekle
            if (!context.Products.Any())
            {
                var products = new List<EticaretApp.Models.Product>
                {
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Laptop", 
                        Description = "Yüksek performanslı laptop", 
                        Price = 15000.00m, 
                        Stock = 10, 
                        CategoryId = elektronikId,
                        ImageUrl = "/images/laptop.jpg",
                        CreatedDate = DateTime.Now
                    },
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Akıllı Telefon", 
                        Description = "Son model akıllı telefon", 
                        Price = 12000.00m, 
                        Stock = 15, 
                        CategoryId = elektronikId,
                        ImageUrl = "/images/phone.jpg",
                        CreatedDate = DateTime.Now
                    },
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Tişört", 
                        Description = "Rahat pamuklu tişört", 
                        Price = 150.00m, 
                        Stock = 50, 
                        CategoryId = giyimId,
                        ImageUrl = "/images/tshirt.jpg",
                        CreatedDate = DateTime.Now
                    },
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Pantolon", 
                        Description = "Klasik kesim pantolon", 
                        Price = 300.00m, 
                        Stock = 30, 
                        CategoryId = giyimId,
                        ImageUrl = "/images/pants.jpg",
                        CreatedDate = DateTime.Now
                    },
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Masa Lambası", 
                        Description = "LED masa lambası", 
                        Price = 250.00m, 
                        Stock = 20, 
                        CategoryId = evYasamId,
                        ImageUrl = "/images/lamp.jpg",
                        CreatedDate = DateTime.Now
                    },
                    new EticaretApp.Models.Product 
                    { 
                        Name = "Yatak Örtüsü", 
                        Description = "Pamuklu yatak örtüsü takımı", 
                        Price = 400.00m, 
                        Stock = 25, 
                        CategoryId = evYasamId,
                        ImageUrl = "/images/bedding.jpg",
                        CreatedDate = DateTime.Now
                    }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }

        // Rolleri oluştur
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Varsayılan admin kullanıcısı oluştur veya güncelle
        try
        {
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!";
            
            // Önce rolleri oluştur (eğer yoksa)
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRoleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
                if (adminRoleResult.Succeeded)
                {
                    logger.LogInformation("Admin rolü oluşturuldu");
                }
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                var userRoleResult = await roleManager.CreateAsync(new IdentityRole("User"));
                if (userRoleResult.Succeeded)
                {
                    logger.LogInformation("User rolü oluşturuldu");
                }
            }
            
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
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
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("✅ Admin kullanıcısı başarıyla oluşturuldu: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("❌ Admin kullanıcısı oluşturulamadı. Hatalar: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Admin kullanıcısı varsa şifresini kesin olarak sıfırla
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                if (resetResult.Succeeded)
                {
                    logger.LogInformation("✅ Admin kullanıcısının şifresi sıfırlandı: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("❌ Admin şifresi sıfırlanamadı. Hatalar: {Errors}", 
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
                
                // EmailConfirmed'i true yap
                if (!adminUser.EmailConfirmed)
                {
                    adminUser.EmailConfirmed = true;
                    var updateResult = await userManager.UpdateAsync(adminUser);
                    if (updateResult.Succeeded)
                    {
                        logger.LogInformation("Admin kullanıcısının email'i onaylandı");
                    }
                }
                
                // Lockout'u kaldır
                if (adminUser.LockoutEnabled)
                {
                    adminUser.LockoutEnabled = false;
                    adminUser.LockoutEnd = null;
                    await userManager.UpdateAsync(adminUser);
                }
                
                // Admin rolünü kontrol et ve ekle
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Admin rolü kullanıcıya eklendi: {Email}", adminEmail);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin kullanıcısı oluşturulurken beklenmeyen bir hata oluştu");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication ve Authorization middleware'leri (sıra önemli!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
