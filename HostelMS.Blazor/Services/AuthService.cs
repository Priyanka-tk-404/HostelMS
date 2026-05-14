using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using HostelMS.Blazor.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace HostelMS.Blazor.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _storage;
        private readonly HttpClient _http;

        public JwtAuthStateProvider(ILocalStorageService storage, HttpClient http)
        {
            _storage = storage;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _storage.GetItemAsStringAsync("auth_token");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    await _storage.RemoveItemAsync("auth_token");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                AttachToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void AttachToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task NotifyLogin(string token)
        {
            await _storage.SetItemAsStringAsync("auth_token", token);
            AttachToken(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task NotifyLogout()
        {
            await _storage.RemoveItemAsync("auth_token");
            await _storage.RemoveItemAsync("auth_user");
            _http.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public interface IAuthService
    {
        Task<(bool success, string message)> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<AuthResponseDto?> GetCurrentUserAsync();
        Task EnsureTokenAttached();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;
        private readonly JwtAuthStateProvider _authProvider;

        public AuthService(HttpClient http, ILocalStorageService storage, AuthenticationStateProvider authProvider)
        {
            _http = http;
            _storage = storage;
            _authProvider = (JwtAuthStateProvider)authProvider;
        }

        public async Task EnsureTokenAttached()
        {
            var token = await _storage.GetItemAsStringAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
                _authProvider.AttachToken(token);
        }

        public async Task<(bool success, string message)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", new LoginDto(email, password));
                if (!response.IsSuccessStatusCode)
                    return (false, "Invalid email or password");
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (result == null) return (false, "Login failed");
                await _storage.SetItemAsync("auth_user", result);
                await _authProvider.NotifyLogin(result.Token);
                return (true, "Login successful");
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task LogoutAsync() => await _authProvider.NotifyLogout();
        public async Task<AuthResponseDto?> GetCurrentUserAsync()
            => await _storage.GetItemAsync<AuthResponseDto>("auth_user");
    }
}
