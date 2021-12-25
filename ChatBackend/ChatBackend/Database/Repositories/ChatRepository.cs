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
    public class ChatRepository : IChatRepository
    {
        private readonly DatabaseContext dbContext;

        public ChatRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<int> CountDialogs(int userId)
        {
            return await dbContext.UsersDialogs
                .AsNoTracking()
                .Include(x => x.Dialog.Messages.LastOrDefault())
                .Where(x => x.UserId == userId && !x.Dialog.Messages[0].IsRead)
                .CountAsync();
        }

        public async Task CreateMessage(Message message)
        {
            await dbContext.Messages.AddAsync(message);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteDialog(int dialogId)
        {
            var dialog = await dbContext.Dialogs.FindAsync(dialogId);

            if (dialog is null)
            {
                return;
            }

            dbContext.Dialogs.Remove(dialog);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteMessage(Message message)
        {
            dbContext.Messages.Remove(message);
            await dbContext.SaveChangesAsync();
        }

        public async Task<UserDialog[]> GetUsersDialog(int dialogId)
        {
            return await dbContext.UsersDialogs.Where(x => x.DialogId == dialogId).ToArrayAsync();
        }

        public async Task<Dialog> CreateDialog(Dialog dialog)
        {
            var check = await dbContext.Dialogs.FindAsync(dialog);
            if (check is not null)
            {
                return check;
            }

            await dbContext.Dialogs.AddAsync(dialog);
            await dbContext.SaveChangesAsync();

            return dialog;
        }

        public async Task AddUsersDialog(int[] usersId, int dialogId)
        {
            for (int i = 0; i < usersId.Length; i++)
            {
                var userDialog = new UserDialog() { UserId = usersId[i], DialogId = dialogId };
                await dbContext.UsersDialogs.AddAsync(userDialog);
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task<Dialog> GetDialog(int userId, int toUserId)
        {
            var dialogs = await dbContext.UsersDialogs
                .Include(x => x.Dialog)
                .Where(x => x.UserId == userId && !x.Dialog.IsMultiple)
                .ToListAsync();

            UserDialog dialog = null;
            for (int i = 0; i < dialogs.Count; i++)
            {
                dialog = await dbContext.UsersDialogs
                .Include(x => x.Dialog)
                .Where(x => x.UserId == toUserId && x.DialogId == dialogs[i].DialogId)
                .FirstOrDefaultAsync();

                if (dialog != null)
                {
                    break;
                }
            }

            if (dialog == null)
            {
                var user = await dbContext.Users
                    .Where(x => x.Id == toUserId)
                    .Select(x => new { x.Login, x.Image })
                    .FirstOrDefaultAsync();

                var newDialog = new Dialog()
                {
                    Name = user.Login,
                    Image = user.Image,
                    UserId = userId,
                    IsMultiple = false
                };
                await dbContext.Dialogs.AddAsync(newDialog);
                await dbContext.SaveChangesAsync();
                await dbContext.UsersDialogs.AddAsync(new UserDialog { UserId = userId, DialogId = newDialog.Id });
                await dbContext.UsersDialogs.AddAsync(new UserDialog { UserId = toUserId, DialogId = newDialog.Id });
                await dbContext.SaveChangesAsync();

                return newDialog;
            }

            return dialog.Dialog;
        }

        public async Task CheckDialog(int id)
        {
            var message = await dbContext.Messages
                .Where(x => x.DialogId == id && !x.IsRead)
                .ToListAsync();

            for (int i = 0; i < message.Count; i++)
            {
                message[i].IsRead = true;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Message>> GetMessages(int dialogId)
        {
            return await dbContext.Messages
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DialogModel>> GetDialogs(int userId)
        {
            return await dbContext.UsersDialogs
                .AsNoTracking()
                .Include(x => x.Dialog)
                .Include(x => x.Dialog.Messages.LastOrDefault())
                .Where(x => x.UserId == userId)
                .Select(x => new DialogModel
                {
                    Id = x.DialogId,
                    Login = x.Dialog.Name,
                    IsConfirm = x.Dialog.Messages[0].IsRead,
                    DateTime = x.Dialog.Messages[0].DateCreate,
                    LastMessage = x.Dialog.Messages[0].Content,
                    LastUserId = x.Dialog.Messages[0].UserId
                })
                .ToListAsync();
        }
    }
}