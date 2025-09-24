using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Server.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;
using System.Security.Claims;
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

            var windowsIdentity = ClaimsPrincipal.Current?.Identity;
            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var loginName = windowsIdentity.Name?.Split('\\').Last();
            var user = await _userService.GetUserByLoginAsync(loginName ?? "");

            var claimsIdentity = new ClaimsIdentity(windowsIdentity);

            if (user != null)
            {
                claimsIdentity.AddClaim(new Claim("DisplayName", user.Name ?? ""));
                if (user.Permission != null)
                {
                    var permissions = user.Permission.Split(',').Select(p => p.Trim());
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