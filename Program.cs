using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register ApplicationDbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Show database-related error pages during development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity with password rules, lockout settings, and role support
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // No email confirmation required for this student project
    options.SignIn.RequireConfirmedAccount = false;

    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lock user account after 5 failed login attempts for 5 minutes
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>() // Enable role management
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC controller and view support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Must come before Authorization
app.UseAuthorization();

// Create default roles and admin user when app starts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Get RoleManager and UserManager services from DI container
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Define project roles
    string[] roles = { "Admin", "Manager", "Customer" };

    // Create roles if they do not already exist
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Default admin account for testing
    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin123";

    // Check if admin user already exists
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        // Create admin user
        var result = await userManager.CreateAsync(user, adminPassword);

        // Assign Admin role if creation succeeds
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Identity Razor pages like Login/Register
app.MapRazorPages();

app.Run();