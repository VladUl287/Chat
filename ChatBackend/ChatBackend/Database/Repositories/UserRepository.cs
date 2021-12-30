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
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext dbContext;

        public UserRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Delete(User user)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task<User> Get(int id)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return user;
        }

        public async Task<UserPageModel> Get(int authId, int id)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .Select(x => new UserPageModel
                {
                    Id = x.Id,
                    Email = x.Email,
                    Image = x.Image,
                    Login = x.Login
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user is not null)
            {
                var exists = await dbContext.Friends
                    .FirstOrDefaultAsync(x => x.UserId == authId && x.ToUserId == id);

                if (exists is not null)
                {
                    if (exists.IsConfirmed)
                    {
                        user.IsFriend = true;
                    }
                    else
                    {
                        user.IsReceiver = true;
                    }
                }
                else
                {
                    var exists2 = await dbContext.Friends
                        .FirstOrDefaultAsync(x => x.UserId == id && x.ToUserId == authId);

                    if (exists2 is not null)
                    {
                        if (exists2.IsConfirmed)
                        {
                            user.IsFriend = true;
                        }
                        else
                        {
                            user.IsSender = true;
                        }
                    }
                }
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
            var result = await dbContext.Users
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
            //.Where(e => e.Login.Contains(login))
            //.Where(e => EF.Functions.Like(e.Login, "[aei%"))

            return result;
        }
    }
}