using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<LocalImageStorage>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<SecurityStampValidatorOptions>(options => {
    // Blocked users and role changes take effect within a minute instead of the default 30.
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

var app = builder.Build();

await IdentitySeed.SeedAsync(app.Services);
if (app.Configuration.GetValue("SeedDemoData", true)) {
    await DemoDataSeed.SeedAsync(app.Services);
}

if (app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
}
else {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
