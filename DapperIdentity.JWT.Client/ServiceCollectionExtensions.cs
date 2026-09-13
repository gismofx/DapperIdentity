using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace CPE.DapperIdentity.JWT.Client.Services;

/// <summary>
/// Extension methods for registering DapperIdentity JWT client services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the server-side JWT HTTP client (<see cref="JWTAuthClient"/>)
    /// for use in non-WASM contexts where localStorage and interceptors are not needed.
    /// </summary>
    public static IServiceCollection AddJWTAuthClient(this IServiceCollection services)
    {
        services.AddTransient<JWTAuthClient>();
        return services;
    }

    /// <summary>
    /// Registers the full JWT authentication stack for Blazor WebAssembly applications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="JWTWasmClient"/> is registered as <c>Scoped</c> so its in-memory token
    /// cache survives for the full browser-tab lifetime. In Blazor WASM (single user per tab),
    /// scoped is effectively a singleton per circuit. A transient registration would create a
    /// new instance per injection, losing the cache on every resolve.
    /// </para>
    /// <para>
    /// Token validation and refresh logic lives directly on <see cref="JWTWasmClient"/> via
    /// <c>GetValidTokenAsync()</c>. <see cref="HttpInterceptorService"/> calls it directly —
    /// no intermediate service layer needed.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddJWTWasmClient(this IServiceCollection services)
    {
        services.AddJWTAuthClient();
        services.AddScoped<JWTWasmClient>();
        services.AddHttpClientInterceptor();
        services.AddScoped<HttpInterceptorService>();
        return services;
    }
}
