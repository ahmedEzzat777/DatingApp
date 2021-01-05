using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
        {
            IQueryable<AppUser> users = null;

            var likes =
                _context.Likes
                    .Include(l => l.LikedUser).ThenInclude(user => user.Photos)
                    .Include(l => l.SourceUser).ThenInclude(user => user.Photos)
                    .AsQueryable();

            switch (likesParams.Predicate)
            {
                case "liked":
                    users = likes.Where(like => like.SourceUserId == likesParams.UserId)
                                 .Select(like => like.LikedUser);
                    break;
                case "likedBy":
                    users = likes.Where(like => like.LikedUserId == likesParams.UserId)
                                 .Select(like => like.SourceUser);
                    break;
                default:
                    return null;
            }

            var result = users?.Select(user => new LikeDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                UserId = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikesAsync(int userId)
        {
            return await _context.Users
                .Include(user => user.LikedUsers)
                .Include(user => user.LikedByUsers)
                .FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}