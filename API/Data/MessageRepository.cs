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
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container.ToLower() switch
            {
                "inbox" => query.Where(u => u.RecipientUsername == messageParams.Username 
                    && u.RecipientDeleted == false),
                "outbox" => query.Where(u => u.SenderUsername == messageParams.Username 
                    && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username 
                    && u.RecipientDeleted == false
                    && u.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<PagedList<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername, int pageNumber, int pageSize)
        {
            var messages = _context.Messages
                .Include(m => m.Sender).ThenInclude(p => p.Photos)
                .Include(m => m.Recipient).ThenInclude(p => p.Photos)
                .Where(m => 
                    m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername && m.RecipientDeleted == false
                    || m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && m.SenderDeleted == false)
                .OrderBy(m => m.MessageSent);

            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername);

            if(await unreadMessages.AnyAsync())
            {
                foreach(var message in await unreadMessages.ToListAsync())
                    message.DateRead = DateTime.UtcNow;
            }
            
            var query = messages.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            
            return await PagedList<MessageDto>.CreateFromBottomAsync(query, pageNumber, pageSize);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}