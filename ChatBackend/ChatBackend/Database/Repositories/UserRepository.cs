using ChatAppServer.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Database.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext dbContext;

        public UserRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<UserModel> Get(int authId, int id)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .Select(x => new UserModel
                {
                    Id = x.Id,
                    Email = x.Email,
                    Image = x.Image,
                    Login = x.Login
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user is not null)
            {
                //union
                var exists = await dbContext.Friends
                    .AsNoTracking()
                    .Where(x => (x.UserId == authId && x.ToUserId == id) || (x.UserId == id && x.ToUserId == authId))
                    .Select(x => new { x.UserId, x.IsConfirmed })
                    .FirstOrDefaultAsync();

                if (exists is not null)
                {
                    if (exists.IsConfirmed)
                    {
                        user.IsFriend = true;
                    }
                    else if (exists.UserId == id)
                    {
                        user.IsReceiver = true;
                    }
                    else if (exists.UserId != id)
                    {
                        user.IsSender = true;
                    }
                }

                //var receiver = await dbContext.Friends
                //    .AsNoTracking()
                //    .Where(x => x.UserId == authId && x.ToUserId == id)
                //    .Select(x => new { x.IsConfirmed })
                //    .FirstOrDefaultAsync();

                //if (receiver is not null)
                //{
                //    if (receiver.IsConfirmed)
                //    {
                //        user.IsFriend = true;
                //    }
                //    else
                //    {
                //        user.IsReceiver = true;
                //    }
                //}
                //else
                //{
                //    var sender = await dbContext.Friends
                //        .AsNoTracking()
                //        .Where(x => x.UserId == id && x.ToUserId == authId)
                //        .Select(x => new { x.IsConfirmed })
                //        .FirstOrDefaultAsync();

                //    if (sender is not null)
                //    {
                //        if (sender.IsConfirmed)
                //        {
                //            user.IsFriend = true;
                //        }
                //        else
                //        {
                //            user.IsSender = true;
                //        }
                //    }
                //}
            }

            return user;
        }

        public async Task<IEnumerable<UserModel>> GetAll(int id)
        {
            var result = await dbContext.Users
                .AsNoTracking()
                .Select(e => new UserModel
                {
                    Id = e.Id,
                    Email = e.Email,
                    Login = e.Login,
                    Image = e.FacialImage
                })
                .Where(e => e.Id != id)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<UserModel>> Search(int id, string login)
        {
            return await dbContext.Users
                .AsNoTracking()
                .Select(x => new UserModel
                {
                    Id = x.Id,
                    Email = x.Email,
                    Login = x.Login,
                    Image = x.FacialImage
                })
                .Where(x => x.Login.StartsWith(login) && x.Id != id)
                .ToListAsync();
        }

        public async Task Delete(int id)
        {
            await dbContext.Database
                .ExecuteSqlInterpolatedAsync($"DELETE FROM [Users] WHERE [Id] = {id}");
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}