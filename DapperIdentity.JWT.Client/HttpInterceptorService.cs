using System.Net.Http.Headers;
using Toolbelt.Blazor;

namespace CPE.DapperIdentity.JWT.Client;

/// <summary>
/// Intercepts all outgoing <see cref="HttpClient"/> requests and stamps a valid
/// JWT bearer token onto the <c>Authorization</c> header before the request is sent.
/// </summary>
/// <remarks>
/// <para>
/// This is the single authoritative location for HTTP auth header management.
/// Token retrieval and refresh logic is delegated entirely to <see cref="JWTWasmClient"/>,
/// which is the sole localStorage gatekeeper and in-memory token cache.
/// </para>
/// <para>
/// If no token is available (user is on the login page or logged out), the header is
/// not set and the request proceeds unauthenticated, which is the correct behavior.
/// </para>
/// </remarks>
public class HttpInterceptorService
{
    private readonly HttpClientInterceptor _interceptor;
    private readonly JWTWasmClient _jwtClient;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpInterceptorService"/>.
    /// </summary>
    /// <param name="interceptor">The shared HTTP client interceptor from Toolbelt.</param>
    /// <param name="jwtClient">
    /// The JWT lifecycle manager. Provides the current valid token, refreshing
    /// proactively when near expiry.
    /// </param>
    public HttpInterceptorService(HttpClientInterceptor interceptor, JWTWasmClient jwtClient)
    {
        _interceptor = interceptor;
        _jwtClient = jwtClient;
    }

    /// <summary>
    /// Registers the before-send handler on the shared <see cref="HttpClientInterceptor"/>.
    /// Call this once during component initialization (e.g., <c>OnInitializedAsync</c>).
    /// </summary>
    public void RegisterEvent() => _interceptor.BeforeSendAsync += InterceptBeforeHttpAsync;

    /// <summary>
    /// Stamps a valid JWT bearer token onto every non-auth outgoing request.
    /// Skips requests whose path contains <c>jwtauth</c> to avoid circular dependency
    /// with the token acquisition flow itself (login, refresh, forgot-password).
    /// </summary>
    /// <param name="sender">The <see cref="HttpClientInterceptor"/> that raised the event.</param>
    /// <param name="e">Event args containing the outgoing <see cref="System.Net.Http.HttpRequestMessage"/>.</param>
    public async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        if (e.Request.RequestUri.AbsolutePath.Contains("jwtauth")) return;

        var token = await _jwtClient.GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
            e.Request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
    }

    /// <summary>
    /// Unregisters the before-send handler. Call during component disposal
    /// (<see cref="IDisposable.Dispose"/>) to prevent memory leaks.
    /// </summary>
    public void DisposeEvent() => _interceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
}
