using System.Collections.Generic;
using System.Threading.Tasks;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Logic
{
    public interface IUserMasterRepository
    {
        Task<MY_SHOP_APP_API.Models.PagedResult<UserMaster>> GetAllAsync(int pageNumber, int pageSize, string? search, bool onlyActive);
        Task<UserMaster?> GetByIdAsync(int id);
        Task<UserMaster?> GetByPhoneOrEmailAsync(string? phone, string? email);
        Task<UserMaster?> GetByPhoneAndEmailAsync(string phone, string email);
        Task<UserMaster> AddAsync(UserMaster entity);
        Task UpdateAsync(UserMaster entity);
        Task DeleteAsync(int id);
    }
}
