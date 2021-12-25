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
            return await dbContext.Users.FindAsync(id);
        }

        public async Task<IEnumerable<UserModel>> GetAll(int id)
        {
            var friends = await dbContext.Friends
               .AsNoTracking()
               .Where(x => x.UserId == id)
               .Select(x => x.ToUserId)
               .ToListAsync();

            var friends2 = await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.ToUserId == id)
                .Select(x => x.UserId)
                .ToListAsync();

            var allFriends = new List<int>(friends.Count + friends2.Count);

            allFriends.AddRange(friends);
            allFriends.AddRange(friends2);

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

            for (int i = 0; i < allFriends.Count; i++)
            {
                var index = result.FindIndex(x => x.Id == allFriends[i]);
                if (index > -1)
                {
                    result[index].IsFriend = true;
                }
            }

            return result;
        }

        public async Task<IEnumerable<UserModel>> Search(int id, string login)
        {
            var friends = await dbContext.Friends
              .AsNoTracking()
              .Where(x => x.UserId == id)
              .Select(x => x.ToUserId)
              .ToListAsync();

            var friends2 = await dbContext.Friends
                .AsNoTracking()
                .Where(x => x.ToUserId == id)
                .Select(x => x.UserId)
                .ToListAsync();

            var allFriends = new List<int>(friends.Count + friends2.Count);

            allFriends.AddRange(friends);
            allFriends.AddRange(friends2);

            var result = await dbContext.Users
                .AsNoTracking()
                .Select(e => new UserModel
                {
                    Id = e.Id,
                    Email = e.Email,
                    Login = e.Login,
                    Image = e.FacialImage
                })
                .Where(e => e.Login.Contains(login))
                .ToListAsync();

            for (int i = 0; i < allFriends.Count; i++)
            {
                var index = result.FindIndex(x => x.Id == allFriends[i]);
                if (index > -1)
                {
                    result[index].IsFriend = true;
                }
            }

            return result;
        }
    }
}