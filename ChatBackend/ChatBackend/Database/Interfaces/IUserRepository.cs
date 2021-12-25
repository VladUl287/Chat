using ChatAppModels;
using ChatBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAll(int id);

        Task<IEnumerable<UserModel>> Search(int id, string login);

        Task<User> Get(int id);

        Task Delete(User user);
    }
}