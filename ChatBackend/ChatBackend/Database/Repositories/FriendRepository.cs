using ChatAppModels;
using ChatAppServer.Interfaces;
using ChatBackend.Database;
using ChatBackend.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAppServer.Repositories
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
            var friends = await dbContext.Friends
                .AsNoTracking()
                .Include(x => x.ToUser)
                .Where(x => x.UserId == id && x.IsConfirmed)
                .Select(x => new UserModel 
                { 
                    Id = x.ToUserId, 
                    Login = x.ToUser.Login, 
                    Email = x.ToUser.Email, 
                    Image = x.ToUser.FacialImage, 
                    IsFriend = true 
                })
                .ToListAsync();

            var friends2 = await dbContext.Friends
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.ToUserId == id && x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.UserId,
                    Login = x.User.Login,
                    Email = x.User.Email,
                    Image = x.User.FacialImage,
                    IsFriend = true
                })
                .ToListAsync();

            friends.AddRange(friends2);

            return friends;
        }

        public async Task<IEnumerable<UserModel>> GetIncoming(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.ToUserId == id && !x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.UserId,
                    Login = x.User.Login,
                    Email = x.User.Email,
                    Image = x.User.FacialImage,
                    IsFriend = true
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> GetOutgoing(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Include(x => x.ToUser)
                .Where(x => x.UserId == id && !x.IsConfirmed)
                .Select(x => new UserModel
                {
                    Id = x.ToUserId,
                    Login = x.ToUser.Login,
                    Email = x.ToUser.Email,
                    Image = x.ToUser.FacialImage,
                    IsFriend = true
                })
                .ToListAsync();
        }

        public async Task Accept(int id, int fromId)
        {
            var friend = await dbContext.Friends.FirstOrDefaultAsync(e => e.ToUserId == id && e.UserId == fromId);

            if (friend is not null)
            {
                friend.IsConfirmed = true;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetCountIncoming(int id)
        {
            return await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.ToUserId == id && !x.IsConfirmed)
                .CountAsync();
        }
    }
}