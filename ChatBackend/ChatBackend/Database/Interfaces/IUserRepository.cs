using ChatBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAll(int id);

        Task<IEnumerable<UserModel>> Search(int id, string login);

        Task<UserModel> Get(int userId, int id);

        Task Delete(int id);

        Task<int> SaveChangesAsync();
    }
}