using Microsoft.AspNetCore.Authentication.Cookies;
using WebPasteleria.Configuration;
using WebPasteleria.Services.Administracion;
using WebPasteleria.Services.Catalogo;
using WebPasteleria.Services.Notificaciones;
using WebPasteleria.Services.Pedidos;
using WebPasteleria.Services.Reportes;
using WebPasteleria.Services.Security;

namespace WebPasteleria.Extensions;

/// <summary>Punto de composición: aquí (y solo aquí) se enlazan las abstracciones con sus implementaciones.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPasteleriaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EnvioOptions>(configuration.GetSection(EnvioOptions.Seccion));
        services.Configure<PedidoOptions>(configuration.GetSection(PedidoOptions.Seccion));
        services.Configure<SeguridadOptions>(configuration.GetSection(SeguridadOptions.Seccion));
        services.Configure<CorreoOptions>(configuration.GetSection(CorreoOptions.Seccion));
        return services;
    }

    public static IServiceCollection AddPasteleriaAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var seguridad = configuration.GetSection(SeguridadOptions.Seccion).Get<SeguridadOptions>() ?? new SeguridadOptions();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccesoDenegado";
                options.Cookie.Name = "Pasteleria.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(seguridad.MinutosSesion);
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddPasteleriaServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        // Seguridad
        services.AddSingleton<ILegacyPasswordVerifier, LegacyAesPasswordVerifier>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();

        // Pedidos
        services.AddSingleton<IShippingCostCalculator, ShippingCostCalculator>();
        services.AddSingleton<ITrackingCodeGenerator, TrackingCodeGenerator>();
        services.AddScoped<IPedidoCheckoutService, PedidoCheckoutService>();
        services.AddScoped<PedidoConsultaService>();
        services.AddScoped<IPedidoConsultaPublica>(sp => sp.GetRequiredService<PedidoConsultaService>());
        services.AddScoped<IPedidoConsultaAdmin>(sp => sp.GetRequiredService<PedidoConsultaService>());
        services.AddScoped<IPedidoEstadoService, PedidoEstadoService>();

        // Catálogo público
        services.AddScoped<ICatalogoService, CatalogoService>();

        // Mantenimientos del panel (una misma clase puede exponer su contrato y su lookup)
        services.AddScoped<CategoriaAdminService>();
        services.AddScoped<ICategoriaAdminService>(sp => sp.GetRequiredService<CategoriaAdminService>());
        services.AddScoped<ICategoriaLookup>(sp => sp.GetRequiredService<CategoriaAdminService>());
        services.AddScoped<EmpleadoAdminService>();
        services.AddScoped<IEmpleadoAdminService>(sp => sp.GetRequiredService<EmpleadoAdminService>());
        services.AddScoped<IEmpleadoLookup>(sp => sp.GetRequiredService<EmpleadoAdminService>());
        services.AddScoped<IProductoAdminService, ProductoAdminService>();
        services.AddScoped<IClienteAdminService, ClienteAdminService>();
        services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();

        // Reportes y notificaciones
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
