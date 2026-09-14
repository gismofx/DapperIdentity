using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;
// This class sits in Microsoft.Extensions.DependencyInjection so AddJwtWasmClient is discoverable
// from a consumer's Program.cs with no using at all, so the library's own types need importing.
using CPE.DapperIdentity.Jwt.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering DapperIdentity JWT client services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the server-side JWT HTTP client (<see cref="JwtAuthClient"/>)
    /// for use in non-WASM contexts where localStorage and interceptors are not needed.
    /// </summary>
    public static IServiceCollection AddJwtAuthClient(this IServiceCollection services)
    {
        services.AddTransient<JwtAuthClient>();
        return services;
    }

    /// <summary>
    /// Registers the full JWT authentication stack for Blazor WebAssembly applications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="JwtWasmClient"/> is registered as <c>Scoped</c> so its in-memory token
    /// cache survives for the full browser-tab lifetime. In Blazor WASM (single user per tab),
    /// scoped is effectively a singleton per circuit. A transient registration would create a
    /// new instance per injection, losing the cache on every resolve.
    /// </para>
    /// <para>
    /// Token validation and refresh logic lives directly on <see cref="JwtWasmClient"/> via
    /// <c>GetValidTokenAsync()</c>. <see cref="HttpInterceptorService"/> calls it directly —
    /// no intermediate service layer needed.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddJwtWasmClient(this IServiceCollection services)
    {
        services.AddJwtAuthClient();
        services.AddScoped<JwtWasmClient>();
        services.AddHttpClientInterceptor();
        services.AddScoped<HttpInterceptorService>();
        return services;
    }
}
