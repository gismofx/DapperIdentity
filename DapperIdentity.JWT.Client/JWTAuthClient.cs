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

namespace DapperIdentity.JWT.Client
{
    public class JWTAuthClient
    {
        private static string AuthServerSectionName = "AuthServer";
        private static string AuthServerEndpoint = "Endpoint";

        public Uri Endpoint { get; set; }
        
        public JWTAuthClient(IConfiguration configuration)
        {
            var authSection = configuration.GetSection(AuthServerSectionName);
            if (!authSection.Exists()) throw new Exception($"Configuration/AppSettings does not contain section: {AuthServerSectionName}.");

            var address = authSection[AuthServerEndpoint];

            if (string.IsNullOrWhiteSpace(address)) throw new Exception($"{AuthServerEndpoint} property is null/empty");
            Endpoint = new Uri(address);

        }

        public async Task<AuthResponse> Login(AuthRequest userForAuthentication)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = Endpoint;

                var content = JsonSerializer.Serialize(userForAuthentication);
                var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
                var req = new HttpRequestMessage(HttpMethod.Post, "api/jwtauth/login");
                req.Content = bodyContent;

                //client.GenerateCurlInConsole(req);
                //var authResult = await client.PostAsync("api/jwtauth/login", bodyContent);
                
                var authResult = await client.SendAsync(req);
                var authContent = await authResult.Content.ReadAsStringAsync();
                if (!authResult.IsSuccessStatusCode)
                    return new AuthResponse();
                
                var result = JsonSerializer.Deserialize<AuthResponse>(authContent, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }); ;//, _options);
                if (!authResult.IsSuccessStatusCode)
                {
                    return result;
                }


                //return new AuthResponseDto { IsAuthSuccessful = true };
                return result;
                //await _localStorage.SetItemAsync("authToken", result.Token);

                return new AuthResponse();
            }
        }
        public async Task<AuthResponse> Login(string username, string password)
        {
            return await Login(new AuthRequest { Email = username, Password = password }) ;
        }

        public async Task<AuthResponse> RefreshToken(RefreshTokenDto refreshObject)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = Endpoint;

                var content = JsonSerializer.Serialize(refreshObject);
                var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
                var req = new HttpRequestMessage(HttpMethod.Post, "api/jwtauth/refresh");
                req.Content = bodyContent;

                //client.GenerateCurlInConsole(req);
                //var authResult = await client.PostAsync("api/jwtauth/login", bodyContent);

                var authResult = await client.SendAsync(req);
                if (!authResult.IsSuccessStatusCode)
                { 
                    return  new AuthResponse(); 
                }
                var authContent = await authResult.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(authContent, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }); ;//, _options);
                return result;
                /*
                if (!authResult.IsSuccessStatusCode)
                {
                    return result;
                }
                */
            }
        }
        
    }
}
