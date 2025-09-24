using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Server.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal; // Required for WindowsIdentity
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Server.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private readonly AppState _appState;

        public CustomAuthenticationStateProvider(IUserService userService, AppState appState)
        {
            _userService = userService;
            _appState = appState;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_appState.IsImpersonating)
            {
                return new AuthenticationState(_appState.ImpersonatedUser!);
            }

            // A more robust way to get the Windows user
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var loginName = windowsIdentity.Name?.Split('\\').LastOrDefault();
            var userFromDb = await _userService.GetUserByLoginAsync(loginName ?? "");

            var claimsIdentity = new ClaimsIdentity(windowsIdentity);

            if (userFromDb != null)
            {
                // Add the user's full name from DB as a display name claim
                claimsIdentity.AddClaim(new Claim("DisplayName", userFromDb.Name ?? ""));

                // Add roles/permissions from the database
                if (!string.IsNullOrEmpty(userFromDb.Permission))
                {
                    var permissions = userFromDb.Permission.Split(',').Select(p => p.Trim());
                    foreach (var permission in permissions)
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, permission));
                    }
                }
            }

            var userPrincipal = new ClaimsPrincipal(claimsIdentity);
            _appState.SetCurrentUser(userPrincipal);
            return new AuthenticationState(userPrincipal);
        }

        public void NotifyStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}