using LE_Digital_2_Blazor_Server_WebApp.Core.Models;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByLoginAsync(string login);
    }
}
