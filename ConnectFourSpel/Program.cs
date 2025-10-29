using Microsoft.AspNetCore.Authentication.Cookies;
using ConnectFourSpel.Services;



var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Vårt in-memory-lager för spel (DI)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.LogoutPath = "/Auth/Logout";
        o.Cookie.Name = "connect4.auth";
        o.Cookie.HttpOnly = true;
        o.SlidingExpiration = true;
        // sätt o.Cookie.SecurePolicy = CookieSecurePolicy.Always vid HTTPS/produktion
        o.Cookie.SameSite = SameSiteMode.Lax;
    });


builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.Name = "connect4.session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

// Prod-fallback
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();               
app.UseAuthentication();       
app.UseAuthorization();

app.UseRouting();


// Standardroute: /{controller}/{action}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

