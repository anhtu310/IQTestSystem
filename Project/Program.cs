using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Repositories;
using Project.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("IQTestSystem");
builder.Services.AddDbContext<IqtestSystemContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login";
        options.AccessDeniedPath = "/Authentication/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    option.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSerivce, UserService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian tồn tại của session
    options.Cookie.HttpOnly = true; // tránh bị script truy cập
    options.Cookie.IsEssential = true; // bắt buộc phải có cookie này
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) =>
{
    var currentPath = context.Request.Path.Value?.ToLower() ?? "";

    // Bỏ qua kiểm tra nếu là trang đăng xuất hoặc các trang xác thực
    if (currentPath.Contains("/authentication/logout") ||
        currentPath.Contains("/authentication/login") ||
        currentPath.Contains("/authentication/accessdenied"))
    {
        await next();
        return;
    }

    var user = context.User;

    if (user.Identity.IsAuthenticated)
    {
        if (user.IsInRole("Admin") && !currentPath.Contains("/admin"))
        {
            context.Response.Redirect("/Admin/Index");
            return;
        }

        if (user.IsInRole("User") && !currentPath.Contains("/home"))
        {
            context.Response.Redirect("/Home/Index");
            return;
        }
    }

    await next();
});

app.Run();
