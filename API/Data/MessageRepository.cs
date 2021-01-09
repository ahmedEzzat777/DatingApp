using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .FirstOrDefaultAsync(g => g.Connections.Any(c => c.ConnectionId == connectionId));
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(m => m.Recipient)
                .Include(m => m.Sender)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(g => g.Connections)
                .FirstOrDefaultAsync(g => g.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container.ToLower() switch
            {
                "inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username 
                    && u.RecipientDeleted == false),
                "outbox" => query.Where(u => u.Sender.UserName == messageParams.Username 
                    && u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username 
                    && u.RecipientDeleted == false
                    && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(p => p.Photos)
                .Include(m => m.Recipient).ThenInclude(p => p.Photos)
                .Where(m => 
                    m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername && m.RecipientDeleted == false
                    || m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && m.SenderDeleted == false)
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead is null && m.Recipient.UserName == currentUsername).ToList();
            
            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                    message.DateRead = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}