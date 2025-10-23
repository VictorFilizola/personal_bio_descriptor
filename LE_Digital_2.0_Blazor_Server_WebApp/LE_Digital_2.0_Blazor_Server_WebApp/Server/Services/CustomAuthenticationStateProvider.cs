using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System; // Add for Console.WriteLine

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
            // Debug: Log current user state
            Console.WriteLine($"GetAuthenticationStateAsync called. User authenticated: {_currentUser.Identity?.IsAuthenticated}, Name: {_currentUser.Identity?.Name}");
            var roles = _currentUser.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            Console.WriteLine($"Roles: {string.Join(", ", roles)}");
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public async Task<bool> LoginAsync(string username)
        {
            Console.WriteLine($"Attempting login for: {username}"); // Log attempt
            var user = await _userService.GetUserByLoginAsync(username);

            if (user == null || string.IsNullOrEmpty(user.Permission))
            {
                Console.WriteLine($"Login FAILED for {username}. User not found or no permissions."); // Log failure
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return false;
            }

            Console.WriteLine($"Login SUCCESS for {username}. User: {user.Name}, Permissions: {user.Permission}"); // Log success

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Login), // Use Login as Name identifier
                new Claim("DisplayName", user.Name ?? "")
            }, "CustomAuth");

            // Add roles from the database
            var permissions = user.Permission.Split(',').Select(p => p.Trim());
            foreach (var permission in permissions)
            {
                Console.WriteLine($"Adding role claim: {permission}"); // Log roles being added
                identity.AddClaim(new Claim(ClaimTypes.Role, permission));
            }

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); // Notify Blazor the user changed
            return true;
        }

        public void Logout()
        {
            Console.WriteLine($"Logging out user: {_currentUser.Identity?.Name}"); // Log logout
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}