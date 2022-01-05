using ChatAppModels;
using ChatBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Interfaces
{
    public interface IFriendRepository
    {
        Task Create(Friend interlocutor);

        Task Accept(int id, int fromId);

        Task Delete(int authId, int userId);

        Task<IEnumerable<UserModel>> GetAll(int id);

        Task<IEnumerable<UserModel>> GetOutgoing(int id);

        Task<IEnumerable<UserModel>> GetIncoming(int id);

        Task<int> GetCount(int id);

        Task<int> SaveChangesAsync();
    }
}