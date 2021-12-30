﻿using ChatAppModels;
using ChatBackend.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppServer.Interfaces
{
    public interface IChatRepository
    {
        Task CheckDialog(int id);

        Task DeleteDialog(int id);

        Task DeleteUserDialog(int id);

        Task<int> CountDialogs(int userId);

        Task CreateMessage(Message message);

        void DeleteMessage(Message message);

        Task DeleteMessages(int[] arrId);

        Task<Dialog> CreateDialog(Dialog dialog);

        Task<int[]> GetUsersDialog(int dialogId);

        Task AddUsersDialog(int[] usersId, int dialogId);

        Task<Dialog> GetDialog(int userId, int toUserId);

        Task<IEnumerable<DialogModel>> GetDialogs(int id);
        
        Task<List<Message>> GetMessages(int dialogId);

        Task<int> SaveChangesAsync();
    }
}