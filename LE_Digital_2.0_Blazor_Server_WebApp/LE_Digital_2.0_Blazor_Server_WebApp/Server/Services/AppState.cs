using System.Security.Claims;

namespace LE_Digital_2_Blazor_Server_WebApp.Server.Services
{
    public class AppState
    {
        public ClaimsPrincipal? CurrentUser { get; private set; }
        public ClaimsPrincipal? ImpersonatedUser { get; private set; }

        public bool IsImpersonating => ImpersonatedUser != null;

        public void SetCurrentUser(ClaimsPrincipal user)
        {
            CurrentUser = user;
        }

        public void StartImpersonation(ClaimsPrincipal userToImpersonate)
        {
            ImpersonatedUser = userToImpersonate;
        }

        public void StopImpersonation()
        {
            ImpersonatedUser = null;
        }
    }
}