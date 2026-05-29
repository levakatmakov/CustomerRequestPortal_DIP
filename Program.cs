using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Data;
using CustomerRequestPortal.Models;

var builder = WebApplication.CreateBuilder(args);

// Подключение базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Identity без требований к паролю
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Инициализация
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    foreach (var role in new[] { AppRoles.Customer, AppRoles.Dispatcher, AppRoles.Executor, AppRoles.Administrator })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@stroysklad.ru";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Администратор",
            Role = AppRoles.Administrator,
            StaffPosition = "Администратор"
        };
        await userManager.CreateAsync(adminUser, "1234");
    }

    adminUser.Role = AppRoles.Administrator;
    adminUser.StaffPosition = "Администратор";
    await userManager.UpdateAsync(adminUser);

    if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Administrator))
        await userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);

    if (await userManager.IsInRoleAsync(adminUser, "Manager"))
    {
        await userManager.RemoveFromRoleAsync(adminUser, "Manager");
    }

    var legacyManagers = userManager.Users
        .Where(u => u.Role == "Manager" && u.Email != adminEmail)
        .ToList();

    foreach (var legacyManager in legacyManagers)
    {
        legacyManager.Role = AppRoles.Dispatcher;
        legacyManager.StaffPosition = "Диспетчер";
        await userManager.UpdateAsync(legacyManager);

        if (!await userManager.IsInRoleAsync(legacyManager, AppRoles.Dispatcher))
            await userManager.AddToRoleAsync(legacyManager, AppRoles.Dispatcher);

        if (await userManager.IsInRoleAsync(legacyManager, "Manager"))
            await userManager.RemoveFromRoleAsync(legacyManager, "Manager");
    }

    await DbSeeder.SeedAsync(db);
}

app.Run();
