using DapperIdentity.JWT.Client;
using Microsoft.AspNetCore.Components.Authorization;

public class RefreshTokenService
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly JWTWasmClient _authClient;//IAuthenticationService _authService;
    public RefreshTokenService(AuthenticationStateProvider authProvider, JWTWasmClient authClient)//IAuthenticationService authService)
    {
        _authProvider = authProvider;
        //_authService = authService;
        _authClient = authClient; 
    }
    public async Task<string> TryRefreshToken()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var exp = user.FindFirst(c => c.Type.Equals("exp")).Value;
        var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
        var timeUTC = DateTime.UtcNow;
        var diff = expTime - timeUTC;
        if (diff.TotalMinutes <= 2)
        {
            var result = await _authClient.RefreshToken();
            return result.Token;
        }
            
        return string.Empty;
    }
}