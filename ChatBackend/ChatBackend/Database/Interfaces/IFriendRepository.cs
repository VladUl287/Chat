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

        Task<IEnumerable<UserModel>> GetAll(int id);

        Task<IEnumerable<UserModel>> GetOutgoing(int id);

        Task<IEnumerable<UserModel>> GetIncoming(int id);

        Task<int> GetCountIncoming(int id);
    }
}