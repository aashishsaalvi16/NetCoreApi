using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MY_SHOP_APP_API.Logic;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Business
{
    public class UserMasterService : IUserMasterService
    {
        private readonly IUserMasterRepository _repository;
        private readonly Microsoft.Extensions.Logging.ILogger<UserMasterService> _logger;

        public UserMasterService(IUserMasterRepository repository, Microsoft.Extensions.Logging.ILogger<UserMasterService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<MY_SHOP_APP_API.Models.PagedResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool onlyActive = true)
        {
            var result = await _repository.GetAllAsync(pageNumber, pageSize, search, onlyActive);

            // map to DTO
            var dtoResult = new MY_SHOP_APP_API.Models.PagedResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = new System.Collections.Generic.List<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>()
            };

            foreach (var item in result.Items)
            {
                dtoResult.Items.Add(MapToDto(item));
            }

            return dtoResult;
        }

        public async Task<MY_SHOP_APP_API.Models.DTOs.UserMasterDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        public async Task<MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> CreateAsync(MY_SHOP_APP_API.Models.DTOs.CreateUserMasterDto model)
        {
            // validate: user with same phone AND email should not already exist
            if (!string.IsNullOrWhiteSpace(model.Phone) && !string.IsNullOrWhiteSpace(model.Email))
            {
                var exists = await _repository.GetByPhoneAndEmailAsync(model.Phone, model.Email);
                if (exists != null)
                {
                    return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Fail("User with same phone and email already exists.");
                }
            }

            var entity = new UserMaster
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                // normalize and store phone/email in lowercase for consistent lookups/indexing
                Phone = model.Phone?.Trim().ToLowerInvariant(),
                Email = model.Email?.Trim().ToLowerInvariant(),
                Address1 = model.Address1,
                Address2 = model.Address2,
                CreatedBy = model.CreatedBy,
                CreatedOn = DateTime.UtcNow,
                IsActive = model.IsActive
            };

            // hash password and set password metadata
            entity.PasswordHash = MY_SHOP_APP_API.Business.PasswordHelper.HashPassword(model.Password);
            entity.PasswordLastChanged = DateTime.UtcNow;
            entity.FailedAttempts = 0;
            entity.LockoutUntil = null;

            var created = await _repository.AddAsync(entity);
            _logger.LogInformation("Created UserMaster with id {UserId}", created.UserId);
            return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Ok(MapToDto(created));
        }

        public async Task<MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> AuthenticateAsync(MY_SHOP_APP_API.Models.DTOs.LoginRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Phone))
                return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Fail("Email or phone is required.");

            // normalize
            var phone = model.Phone?.Trim().ToLowerInvariant();
            var email = model.Email?.Trim().ToLowerInvariant();

            var user = await _repository.GetByPhoneOrEmailAsync(phone, email);
            if (user == null)
                return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Fail("Invalid credentials.");

            // check lockout
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Fail($"Account locked until {user.LockoutUntil.Value:u}");
            }

            var verified = MY_SHOP_APP_API.Business.PasswordHelper.VerifyHashedPassword(user.PasswordHash, model.Password);
            if (!verified)
            {
                // increment failed attempts
                user.FailedAttempts += 1;
                // lockout after 5 failed attempts for 15 minutes
                if (user.FailedAttempts >= 5)
                {
                    user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);
                }
                await _repository.UpdateAsync(user);
                return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Fail("Invalid credentials.");
            }

            // successful login: reset counters
            user.FailedAttempts = 0;
            user.LockoutUntil = null;
            await _repository.UpdateAsync(user);

            return MY_SHOP_APP_API.Models.OperationResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>.Ok(MapToDto(user));
        }

        public async Task<bool> UpdateAsync(int id, MY_SHOP_APP_API.Models.DTOs.UpdateUserMasterDto model)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            existing.FirstName = model.FirstName;
            existing.MiddleName = model.MiddleName;
            existing.LastName = model.LastName;
            // normalize phone/email before storing
            existing.Phone = model.Phone?.Trim().ToLowerInvariant();
            existing.Email = model.Email?.Trim().ToLowerInvariant();
            existing.Address1 = model.Address1;
            existing.Address2 = model.Address2;
            existing.ModifiedOn = DateTime.UtcNow;
            existing.ModifiedBy = model.ModifiedBy;
            existing.IsActive = model.IsActive;

            await _repository.UpdateAsync(existing);
            _logger.LogInformation("Updated UserMaster with id {UserId}", id);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            // soft delete
            existing.IsActive = false;
            existing.ModifiedOn = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            _logger.LogInformation("Soft-deleted UserMaster with id {UserId}", id);
            return true;
        }

        private MY_SHOP_APP_API.Models.DTOs.UserMasterDto MapToDto(UserMaster e)
        {
            return new MY_SHOP_APP_API.Models.DTOs.UserMasterDto
            {
                UserId = e.UserId,
                FirstName = e.FirstName,
                MiddleName = e.MiddleName,
                LastName = e.LastName,
                Phone = e.Phone,
                Email = e.Email,
                Address1 = e.Address1,
                Address2 = e.Address2,
                CreatedOn = e.CreatedOn,
                CreatedBy = e.CreatedBy,
                ModifiedOn = e.ModifiedOn,
                ModifiedBy = e.ModifiedBy,
                IsActive = e.IsActive
            };
        }
    }
}
