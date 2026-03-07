using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MY_SHOP_APP_API.Data;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Logic
{
    public class UserMasterRepository : IUserMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserMasterRepository> _logger;

        public UserMasterRepository(ApplicationDbContext context, ILogger<UserMasterRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<UserMaster>> GetAllAsync(int pageNumber, int pageSize, string? search, bool onlyActive)
        {
            try
            {
                var query = _context.UserMasters.AsNoTracking().AsQueryable();

                if (onlyActive)
                    query = query.Where(u => u.IsActive);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    query = query.Where(u => u.FirstName.Contains(s) || u.LastName.Contains(s) || (u.Email != null && u.Email.Contains(s)));
                }

                var total = await query.CountAsync();
                var ordered = query.OrderBy(u => u.UserId);

                List<UserMaster> items;
                if (pageNumber <= 1)
                {
                    items = await ordered.Take(pageSize).ToListAsync();
                }
                else
                {
                    var take = pageNumber * pageSize;
                    var limited = await ordered.Take(take).ToListAsync();
                    items = limited.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }

                return new PagedResult<UserMaster>
                {
                    Items = items,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAsync (Page {PageNumber}, Size {PageSize})", pageNumber, pageSize);
                throw;
            }
        }

        public async Task<UserMaster?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.UserMasters.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAsync for UserId {UserId}", id);
                throw;
            }
        }

        public async Task<UserMaster?> GetByPhoneOrEmailAsync(string? phone, string? email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                    return null;

                string? phoneNorm = phone?.Trim().ToLowerInvariant();
                string? emailNorm = email?.Trim().ToLowerInvariant();

                return await _context.UserMasters.AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        (phoneNorm != null && u.Phone == phoneNorm) ||
                        (emailNorm != null && u.Email == emailNorm));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByPhoneOrEmailAsync (Phone: {Phone}, Email: {Email})", phone, email);
                throw;
            }
        }

        public async Task<UserMaster?> GetByPhoneAndEmailAsync(string phone, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
                    return null;

                var phoneNorm = phone.Trim().ToLowerInvariant();
                var emailNorm = email.Trim().ToLowerInvariant();

                return await _context.UserMasters.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Phone == phoneNorm && u.Email == emailNorm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByPhoneAndEmailAsync (Phone: {Phone}, Email: {Email})", phone, email);
                throw;
            }
        }

        public async Task<UserMaster> AddAsync(UserMaster entity)
        {
            try
            {
                _context.UserMasters.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync for User {User}", entity);
                throw;
            }
        }

        public async Task UpdateAsync(UserMaster entity)
        {
            try
            {
                _context.UserMasters.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAsync for User {User}", entity);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.UserMasters.FindAsync(id);
                if (entity != null)
                {
                    _context.UserMasters.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAsync for UserId {UserId}", id);
                throw;
            }
        }
    }
}