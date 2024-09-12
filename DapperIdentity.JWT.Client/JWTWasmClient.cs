using DapperIdentity.JWT.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HttpClientToCurl;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DapperIdentity.JWT.Client;
public class JWTWasmClient
{
    private static string AuthServerSectionName = "AuthServer";
    private static string AuthServerEndpoint = "Endpoint";

    private ILocalStorageService _LocalStorage;

    private AuthenticationStateProvider _AuthStateProvider;

    private JWTAuthClient _JWTAuthClient;

    private HttpClient _HttpClient;
        
    public JWTWasmClient(JWTAuthClient jwtWithClient,
                            HttpClient httpClient,
                            AuthenticationStateProvider authStateProvider,
                            ILocalStorageService localStorage)
    {
        _LocalStorage = localStorage;
        _AuthStateProvider = authStateProvider;
        _JWTAuthClient = jwtWithClient;
        _HttpClient = httpClient;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userForAuthentication">User/Password Object to Authenticate</param>
    /// <param name="notifyUserAutentication">Method from AuthStateProvider to invoke NotifyUserAuthentication/NotifyAuthenticationStateChanged</param>
    /// <returns></returns>
    public async Task<AuthResponse> Login(AuthRequest userForAuthentication, Action<string> notifyUserAutentication)
    {
        var result = await _JWTAuthClient.Login(userForAuthentication);
        if (string.IsNullOrWhiteSpace(result.Token)) return result;

        await SetLocalStorage(result);

        notifyUserAutentication(result.Token);
        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
        return result;
    }
        
    /// <summary>
    /// Logout. Remove JWT Token from local storage
    /// </summary>
    /// <param name="notifyUserLogout"></param>
    /// <returns></returns>
    public async Task Logout(Action notifyUserLogout)
    {
        await _LocalStorage.RemoveItemsAsync(new[]{"authToken","refreshToken"});
        notifyUserLogout();
        _HttpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<AuthResponse> RefreshToken()
    {
        var token = await _LocalStorage.GetItemAsync<string>("authToken");
        var refreshToken = await _LocalStorage.GetItemAsync<string>("refreshToken");
        var result = await _JWTAuthClient.RefreshToken(new RefreshTokenDto { Token = token, RefreshToken = refreshToken });

    //var tokenDto = JsonSerializer.Serialize();
    //var bodyContent = new StringContent(tokenDto, Encoding.UTF8, "application/json");
    //var refreshResult = await _client.PostAsync("token/refresh", bodyContent);
    //var refreshContent = await refreshResult.Content.ReadAsStringAsync();
    //var result = JsonSerializer.Deserialize<AuthResponseDto>(refreshContent, _options);
    //if (!refreshResult.IsSuccessStatusCode)
    //    throw new ApplicationException("Something went wrong during the refresh token action");
        await SetLocalStorage(result);

        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
        return result;
    }

    private async Task SetLocalStorage(AuthResponse authResponse)
    {
        await _LocalStorage.SetItemAsync("authToken", authResponse.Token);
        await _LocalStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
    }

    //return await _JWTAuthClient.RefreshToken(refreshTokenDto);
}


