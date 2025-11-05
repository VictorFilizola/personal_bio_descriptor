using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Server.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace LE_Digital_2_Blazor_Server_WebApp.Server.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppState _appState;

        public CustomAuthenticationStateProvider(IUserService userService, IHttpContextAccessor httpContextAccessor, AppState appState)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _appState = appState;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // *** FOR DEVELOPER TESTING ONLY ***
            // Set this to a valid DB login (e.g., "TEIXVIBR") to force login as that user.
            // Set to null to use normal Windows Authentication.
            string forceLoginUser = "ALEXMABR"; // "TEIXVIBR"; 
            // **********************************

            if (!string.IsNullOrEmpty(forceLoginUser))
            {
                // If the developer override is set, bypass all other logic
                return await GetAuthenticationStateForUser(forceLoginUser, isForcedLogin: true);
            }

            // Check for impersonation first
            if (_appState.IsImpersonating && _appState.ImpersonatedUser != null)
            {
                // If we are impersonating, return that user.
                return new AuthenticationState(_appState.ImpersonatedUser);
            }

            // If not impersonating, proceed with Windows Auth
            var windowsUser = _httpContextAccessor.HttpContext?.User;

            if (windowsUser?.Identity?.IsAuthenticated == true)
            {
                var loginName = windowsUser.Identity.Name?.Split('\\').Last();
                if (!string.IsNullOrEmpty(loginName))
                {
                    // Get auth state from DB for the Windows user
                    return await GetAuthenticationStateForUser(loginName, isForcedLogin: false);
                }
            }

            // Not authenticated or user not in DB
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        /// <summary>
        /// Reusable method to get user claims from the database.
        /// </summary>
        private async Task<AuthenticationState> GetAuthenticationStateForUser(string loginName, bool isForcedLogin = false)
        {
            var user = await _userService.GetUserByLoginAsync(loginName);

            if (user != null && !string.IsNullOrEmpty(user.Permission))
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim("DisplayName", user.Name ?? "")
                }, "CustomAuth");

                var permissions = user.Permission.Split(',').Select(p => p.Trim());
                foreach (var permission in permissions)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, permission));
                }

                var appUser = new ClaimsPrincipal(identity);

                // Only set the "real" user in AppState if it's not a forced login
                // and not already impersonating (which this check bypasses)
                if (!isForcedLogin)
                {
                    _appState.SetCurrentUser(appUser);
                }

                return new AuthenticationState(appUser);
            }

            // User not found in DB
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }


        // These methods are now only used for the "Simulate User" feature
        public void StartImpersonation(ClaimsPrincipal impersonatedUser)
        {
            _appState.StartImpersonation(impersonatedUser);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void StopImpersonation()
        {
            _appState.StopImpersonation();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}