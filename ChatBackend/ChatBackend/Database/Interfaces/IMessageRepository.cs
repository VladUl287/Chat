using ChatAppModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Database.Interfaces
{
    public interface IMessageRepository
    {
        Task CreateMessage(Message message);

        void DeleteMessage(Message message);

        Task DeleteMessages(int[] arrId);

        Task<IEnumerable<Message>> GetMessages(int dialogId);

        Task<int> SaveChangesAsync();
    }
}
