using System.Collections.Generic;
using System.Threading.Tasks;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Business
{
    public interface IUserMasterService
    {
        Task<MY_SHOP_APP_API.Models.PagedResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool onlyActive = true);
        Task<MY_SHOP_APP_API.Models.DTOs.UserMasterDto?> GetByIdAsync(int id);
        Task<MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> CreateAsync(MY_SHOP_APP_API.Models.DTOs.CreateUserMasterDto model);
        Task<bool> UpdateAsync(int id, MY_SHOP_APP_API.Models.DTOs.UpdateUserMasterDto model);
        Task<bool> DeleteAsync(int id);
        Task<MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> AuthenticateAsync(MY_SHOP_APP_API.Models.DTOs.LoginRequestDto model);
    }
}
