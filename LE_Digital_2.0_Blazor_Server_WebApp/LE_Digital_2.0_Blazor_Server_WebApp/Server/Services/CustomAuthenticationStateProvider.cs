using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace LE_Digital_2_Blazor_Server_WebApp.Server.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IUserService userService)
        {
            _userService = userService;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public async Task<bool> LoginAsync(string username)
        {
            var user = await _userService.GetUserByLoginAsync(username);

            if (user == null || string.IsNullOrEmpty(user.Permission))
            {
                // User not found or has no permissions, login fails
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return false;
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                // Add other claims as needed, e.g., user's full name
                new Claim("DisplayName", user.Name ?? "")
            }, "CustomAuth");

            // Add roles from the database
            var permissions = user.Permission.Split(',').Select(p => p.Trim());
            foreach (var permission in permissions)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, permission));
            }

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return true;
        }

        public void Logout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}