using Microsoft.EntityFrameworkCore;
using WebPasteleria.Extensions;
using WebPasteleria.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexión 'ConnectionStrings:DefaultConnection'. " +
        "Defínela en appsettings.Development.json, con 'dotnet user-secrets' o en la variable de entorno " +
        "ConnectionStrings__DefaultConnection.");
}

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BakeryContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddPasteleriaOptions(builder.Configuration);
builder.Services.AddPasteleriaAuthentication(builder.Configuration);
builder.Services.AddPasteleriaServices();
builder.Services.AddPasteleriaRateLimiting();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
