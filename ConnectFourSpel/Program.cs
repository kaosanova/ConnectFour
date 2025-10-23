using ConnectFourSpel.Services;
using Microsoft.AspNetCore.Connections;



var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// V�rt in-memory-lager f�r spel (DI)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

// Prod-fallback
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Standardroute: /{controller}/{action}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

