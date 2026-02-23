using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MY_SHOP_APP_API.Data;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Logic
{
    public class UserMasterRepository : IUserMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public UserMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MY_SHOP_APP_API.Models.PagedResult<UserMaster>> GetAllAsync(int pageNumber, int pageSize, string? search, bool onlyActive)
        {
            var query = _context.UserMasters.AsNoTracking().AsQueryable();

            if (onlyActive)
            {
                query = query.Where(u => u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.FirstName.Contains(s) || u.LastName.Contains(s) || (u.Email != null && u.Email.Contains(s)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new MY_SHOP_APP_API.Models.PagedResult<UserMaster>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<UserMaster?> GetByIdAsync(int id)
        {
            return await _context.UserMasters.FindAsync(id);
        }

        public async Task<UserMaster> AddAsync(UserMaster entity)
        {
            _context.UserMasters.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(UserMaster entity)
        {
            _context.UserMasters.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.UserMasters.FindAsync(id);
            if (entity != null)
            {
                _context.UserMasters.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
