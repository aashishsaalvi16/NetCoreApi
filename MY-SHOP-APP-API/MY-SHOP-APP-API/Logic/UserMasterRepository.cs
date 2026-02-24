using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

            // Some SQL Server versions do not support OFFSET/FETCH (used by Skip/Take with OrderBy).
            // To remain compatible with older servers, we avoid generating OFFSET by using Take to limit
            // rows on the server and performing the final Skip in memory when necessary.
            var ordered = query.OrderBy(u => u.UserId);

            List<UserMaster> items;
            if (pageNumber <= 1)
            {
                items = await ordered.Take(pageSize).ToListAsync();
            }
            else
            {
                // fetch up to pageNumber * pageSize rows on server (translated to TOP N), then skip in memory
                var take = pageNumber * pageSize;
                var limited = await ordered.Take(take).ToListAsync();
                items = limited.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

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

        public async Task<UserMaster?> GetByPhoneOrEmailAsync(string? phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                return null;

            // normalize inputs to lower-case; assume phone/email are stored normalized in DB
            string? phoneNorm = phone?.Trim().ToLowerInvariant();
            string? emailNorm = email?.Trim().ToLowerInvariant();

            return await _context.UserMasters.AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    (phoneNorm != null && u.Phone == phoneNorm) ||
                    (emailNorm != null && u.Email == emailNorm));
        }

        public async Task<UserMaster?> GetByPhoneAndEmailAsync(string phone, string email)
        {
            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
                return null;

            var phoneNorm = phone.Trim().ToLowerInvariant();
            var emailNorm = email.Trim().ToLowerInvariant();

            // compare directly assuming DB stores normalized lower-case values
            return await _context.UserMasters.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Phone == phoneNorm && u.Email == emailNorm);
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
