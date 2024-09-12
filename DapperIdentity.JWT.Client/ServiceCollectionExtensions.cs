using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace DapperIdentity.JWT.Client.Services;
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddJWTAuthClient(this IServiceCollection services)
    {
        services.AddTransient<JWTAuthClient>();
        return services;
    }

    public static IServiceCollection AddJWTWasmClient(this IServiceCollection services)
    {
        services.AddJWTAuthClient();
        services.AddTransient<JWTWasmClient>();
        services.AddScoped<RefreshTokenService>();
        services.AddHttpClientInterceptor();
        services.AddScoped<HttpInterceptorService>();
        return services;
    }

}