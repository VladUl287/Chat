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
                .Where(x => 
                    x.UserId == userId && 
                    !x.Dialog.Messages.OrderBy(x => x.DateCreate).LastOrDefault().IsRead && 
                    x.Dialog.Messages.OrderBy(x => x.DateCreate).LastOrDefault().UserId != userId)
                .CountAsync();
        }

        public async Task CreateMessage(Message message)
        {
            await dbContext.Messages.AddAsync(message);
        }

        public async Task DeleteDialog(int dialogId)
        {
            //await dbContext.Database
            //    .ExecuteSqlInterpolatedAsync($"DELETE FROM [Dialogs] WHERE [Id] = {dialogId}");
            var dialog = await dbContext.Dialogs.FindAsync(dialogId);

            if (dialog is null)
            {
                return;
            }

            dbContext.Dialogs.Remove(dialog);
        }

        public async Task DeleteUserDialog(int dialogId)
        {
            //await dbContext.Database
            //    .ExecuteSqlInterpolatedAsync($"DELETE FROM [Dialogs] WHERE [Id] = {dialogId}");
            var dialogs = await dbContext.UsersDialogs
                .Where(x => x.DialogId == dialogId)
                .ToListAsync();

            if (dialogs is null || dialogs.Count == 0)
            {
                return;
            }

            dbContext.UsersDialogs.RemoveRange(dialogs);
        }

        public void DeleteMessage(Message message)
        {
            dbContext.Messages.Remove(message);
        }

        public async Task DeleteMessages(int[] arrId)
        {
            var messagess = await dbContext.Messages
                .Where(x => arrId.Contains(x.Id))
                .ToListAsync();

            dbContext.Messages.RemoveRange(messagess);
        }

        public async Task<int[]> GetUsersDialog(int dialogId)
        {
            return await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .Select(x => x.UserId)
                .ToArrayAsync();
        }

        public async Task<Dialog> CreateDialog(Dialog dialog)
        {
            var check = await dbContext.Dialogs.FirstOrDefaultAsync(x => x.Name == dialog.Name && x.UserId == dialog.UserId);
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

        public async Task CheckDialog(int dialogId)
        {
            await dbContext.Database
                .ExecuteSqlInterpolatedAsync($"UPDATE [Messages] SET [IsRead] = 1 WHERE [DialogId] = {dialogId} AND [IsRead] = 0");
        }

        public async Task<List<Message>> GetMessages(int dialogId)
        {
            return await dbContext.Messages
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DialogModel>> GetDialogs(int userId)
        {
            var dialogsId = await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.Dialog.IsMultiple)
                .Select(x => x.DialogId)
                .ToListAsync();

            var userDialogs = await dbContext.UsersDialogs
                .Where(x => dialogsId.Contains(x.DialogId) && x.UserId != userId)
                .Select(x => new
                {
                    Id = x.DialogId,
                    x.User.Login,
                    Image = x.User.FacialImage,
                    LastMessage = x.Dialog.Messages
                                    .Select(x => new
                                    {
                                        IsConfirm = x.IsRead,
                                        x.DateCreate,
                                        x.Content,
                                        x.UserId
                                    })
                                    .OrderBy(x => x.DateCreate)
                                    .LastOrDefault(),
                })
                .ToListAsync();

            var dialogs = userDialogs
                .Where(x => x.LastMessage != null)
                .Select(x => new DialogModel
                {
                    Id = x.Id,
                    Login = x.Login,
                    Image = x.Image,
                    IsConfirm = x.LastMessage.IsConfirm,
                    DateTime = x.LastMessage.DateCreate,
                    LastMessage = x.LastMessage.Content,
                    LastUserId = x.LastMessage.UserId
                })
                .ToList();

            var userDialogsMult = await dbContext.UsersDialogs
                .Where(x => x.UserId == userId && x.Dialog.IsMultiple)
                .Select(x => new
                {
                    Id = x.DialogId,
                    Login = x.Dialog.Name,
                    Image = x.Dialog.Image,
                    LastMessage = x.Dialog.Messages
                                    .Select(x => new
                                    {
                                        IsConfirm = x.IsRead,
                                        x.DateCreate,
                                        x.Content,
                                        x.UserId
                                    })
                                    .OrderBy(x => x.DateCreate)
                                    .LastOrDefault()
                })
                .ToListAsync();

            if (userDialogsMult.Count > 0)
            {
                var dialogsMult = userDialogsMult
                .Select(x => new DialogModel
                {
                    Id = x.Id,
                    Login = x.Login,
                    Image = x.Image,
                    IsConfirm = x.LastMessage != null && x.LastMessage.IsConfirm,
                    DateTime = x.LastMessage != null ? x.LastMessage.DateCreate : System.DateTime.Now,
                    LastMessage = x.LastMessage != null ? x.LastMessage.Content : string.Empty,
                    LastUserId = x.LastMessage != null ? x.LastMessage.UserId : 0
                });

                dialogs.AddRange(dialogsMult);
            }

            return dialogs;
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}