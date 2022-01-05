using ChatAppModels;
using ChatBackend.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Database.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DatabaseContext dbContext;

        public MessageRepository(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateMessage(Message message)
        {
            await dbContext.Messages.AddAsync(message);
        }

        public void DeleteMessage(Message message)
        {
            dbContext.Messages.Remove(message);
        }

        public async Task DeleteMessages(int[] arrId)
        {
            //await dbContext.Database
            //    .ExecuteSqlInterpolatedAsync($"DELETE FROM [Messages] WHERE [Id] IN ({string.Join(", ", arrId)})");
            var messagess = await dbContext.Messages
                .Where(x => arrId.Contains(x.Id))
                .ToListAsync();

            dbContext.Messages.RemoveRange(messagess);
        }

        public async Task<List<Message>> GetMessages(int dialogId)
        {
            return await dbContext.Messages
                .AsNoTracking()
                .Where(x => x.DialogId == dialogId)
                .ToListAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}
