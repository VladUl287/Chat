using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Database.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly DatabaseContext dbContext;

        public FriendRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Create(Friend friend)
        {
            var exists = await dbContext.Friends
                .AsNoTracking()
                .AnyAsync(x => (x.UserId == friend.UserId && x.ToUserId == friend.ToUserId) ||
                (x.UserId == friend.ToUserId && x.ToUserId == friend.UserId));

            if (!exists)
            {
                await dbContext.Friends.AddAsync(friend);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserModel>> GetAll(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.UserId == id && x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.ToUserId,
                    Login = x.ToUser.Login,
                    Email = x.ToUser.Email,
                    Image = x.ToUser.Image
                })
                .Union(dbContext.Friends
                    .Where(x => x.ToUserId == id && x.IsConfirmed)
                    .Select(x => new UserModel
                    {
                        Id = x.UserId,
                        Login = x.User.Login,
                        Email = x.User.Email,
                        Image = x.User.Image
                    }))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetIncoming(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.ToUserId == id && !x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.UserId,
                    Login = x.User.Login,
                    Email = x.User.Email,
                    Image = x.User.Image
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetOutgoing(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.UserId == id && !x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.ToUserId,
                    Login = x.ToUser.Login,
                    Email = x.ToUser.Email,
                    Image = x.ToUser.Image
                })
                .ToListAsync();
        }

        public async Task Accept(int id, int fromId)
        {
            await dbContext.Database
                .ExecuteSqlInterpolatedAsync($"UPDATE [Friends] SET [IsConfirmed] = 1 WHERE [ToUserId] = {id} AND [UserId] = {fromId}");
        }

        public async Task<int> GetCount(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.ToUserId == id && !x.IsConfirmed)
                .CountAsync();
        }

        public async Task Delete(int authId, int userId)
        {
            var friend = await dbContext.Friends
                .FirstOrDefaultAsync(e => (e.UserId == authId && e.ToUserId == userId) || (e.UserId == userId && e.ToUserId == authId));

            if (friend is not null)
            {
                dbContext.Friends.Remove(friend);
            }
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}