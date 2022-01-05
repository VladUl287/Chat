using ChatAppModels;
using ChatBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Database.Interfaces
{
    public interface IDialogRepository
    {
        Task CheckDialog(int id);

        Task<DialogModel> GetDialogView(int userId, int dialogId);

        Task<IEnumerable<UserModel>> GetUsersDialog(int dialogId);

        Task DeleteDialog(int id);

        Task<int> CountDialogs(int userId);

        Task CreateDialog(Dialog dialog, int[] usersId);

        Task<int[]> GetUsersIdentifier(int dialogId);

        Task<int> GetDialogIdentifier(int userId, int toUserId);

        Task<IEnumerable<DialogModel>> GetDialogs(int id);

        Task<int> SaveChangesAsync();
    }
}
