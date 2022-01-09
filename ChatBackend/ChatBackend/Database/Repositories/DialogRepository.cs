using ChatAppModels;
using ChatBackend.Database.Interfaces;
using ChatBackend.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Database.Repositories
{
    public class DialogRepository : IDialogRepository
    {
        private readonly DatabaseContext dbContext;

        public DialogRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<int> CountDialogs(int userId)
        {
            var query =
               from ud in dbContext.UsersDialogs
               from lm in ud.Dialog.Messages
                   .OrderByDescending(x => x.DateCreate)
                   .Take(1)
                   .DefaultIfEmpty()
               where ud.UserId == userId && !lm.IsRead && lm.UserId != userId
               select ud.DialogId;

            return await query.CountAsync();
        }

        //public async Task DeleteDialog(int dialogId)
        //{
        //    await dbContext.Database
        //        .ExecuteSqlInterpolatedAsync($"DELETE FROM [Dialogs] WHERE [Id] = {dialogId}");
        //}

        public async Task DeleteDialog(int userId, int dialogId)
        {
            var dialog = await dbContext.Dialogs.FindAsync(dialogId);

            if (dialog is not null)
            {
                if (dialog.UserId != userId && dialog.IsMultiple)
                {
                    await dbContext.Database
                        .ExecuteSqlInterpolatedAsync($"DELETE FROM [UsersDialogs] WHERE [UserId] = {userId} AND [DialogId] = {dialogId}");

                    return;
                }

                await dbContext.Database
                    .ExecuteSqlInterpolatedAsync($"DELETE FROM [Dialogs] WHERE [Id] = {dialogId}");
            }
        }

        public async Task<DialogModel> GetDialogView(int userId, int dialogId)
        {
            var dialog = await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId && !x.Dialog.IsMultiple && x.UserId != userId)
                .Select(x => new DialogModel
                {
                    Id = x.UserId,
                    Login = x.User.Login,
                    Image = x.User.Image,
                    IsMultiple = false
                })
                .Union(dbContext.UsersDialogs
                    .Where(x => x.DialogId == dialogId && x.Dialog.IsMultiple)
                    .Select(x => new DialogModel
                    {
                        Id = x.DialogId,
                        Login = x.Dialog.Name,
                        Image = x.Dialog.Image,
                        IsMultiple = true
                    }))
                .FirstOrDefaultAsync();

            return dialog;
        }

        public async Task<int[]> GetUsersIdentifier(int dialogId)
        {
            return await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .Select(x => x.UserId)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<UserModel>> GetUsersDialog(int dialogId)
        {
            return await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .Select(x => new UserModel
                {
                    Id = x.User.Id,
                    Email = x.User.Email,
                    Login = x.User.Login,
                    Image = x.User.Image
                })
                .ToListAsync();
        }

        public async Task CreateDialog(Dialog dialog, int[] usersId)
        {
            using var transaction = dbContext.Database.BeginTransaction();

            var newDialog = await dbContext.Dialogs.AddAsync(dialog);
            await dbContext.SaveChangesAsync();

            for (int i = 0; i < usersId.Length; i++)
            {
                await dbContext.UsersDialogs.AddAsync(new UserDialog { UserId = usersId[i], DialogId = newDialog.Entity.Id });
            }
            await dbContext.SaveChangesAsync();

            transaction.Commit();
        }

        public async Task<int> GetDialogIdentifier(int userId, int toUserId)
        {
            var dialogs = await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.Dialog.IsMultiple)
                .Select(x => x.DialogId)
                .ToListAsync();

            var dialogId = await dbContext.UsersDialogs
                .Where(x => x.UserId == toUserId && dialogs.Contains(x.DialogId))
                .Select(x => x.DialogId)
                .FirstOrDefaultAsync();

            if (dialogId == 0)
            {
                var newDialog = new Dialog()
                {
                    UserId = userId,
                    IsMultiple = false
                };

                await CreateDialog(newDialog, new int[] { userId, toUserId });

                return newDialog.Id;
            }

            return dialogId;
        }

        public async Task CheckDialog(int dialogId)
        {
            await dbContext.Database
                .ExecuteSqlInterpolatedAsync($"UPDATE [Messages] SET [IsRead] = 1 WHERE [DialogId] = {dialogId} AND [IsRead] = 0");
        }

        public async Task<IEnumerable<DialogModel>> GetDialogs(int userId)
        {
            var dialogsId = await dbContext.UsersDialogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.Dialog.IsMultiple)
                .Select(x => x.DialogId)
                .ToListAsync();

            var query =
                from ud in dbContext.UsersDialogs
                from lm in ud.Dialog.Messages
                    .OrderByDescending(x => x.DateCreate)
                    .Take(1)
                    .DefaultIfEmpty()
                where dialogsId.Contains(ud.DialogId) && ud.UserId != userId && lm != null
                select new DialogModel
                {
                    Id = ud.DialogId,
                    Login = ud.User.Login,
                    Image = ud.User.Image,
                    IsConfirm = lm.IsRead,
                    DateTime = lm.DateCreate,
                    LastMessage = lm.Content,
                    LastUserId = lm.UserId
                };

            var queryMultiple =
               from ud in dbContext.UsersDialogs
               from lm in ud.Dialog.Messages
                   .OrderByDescending(x => x.DateCreate)
                   .Take(1)
                   .DefaultIfEmpty()
               where ud.UserId == userId && ud.Dialog.IsMultiple
               select new DialogModel
               {
                   Id = ud.DialogId,
                   Login = ud.Dialog.Name,
                   Image = ud.Dialog.Image,
                   IsConfirm = lm != null && lm.IsRead,
                   DateTime = lm != null ? lm.DateCreate : default,
                   LastMessage = lm != null ? lm.Content : string.Empty,
                   LastUserId = lm != null ? lm.UserId : 0,
               };

            var dialogs = await query.Union(queryMultiple).ToListAsync();

            return dialogs;
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}
