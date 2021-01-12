using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private const string Male = "male";
        private const string Female = "female";
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoService _photoService;
        public UserRepository(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IPhotoService photoService)
        {
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _context = context;
        }

        public async Task<PhotoDto> AddPhoto(IFormFile formFile)
        {
            var user = await GetUserByUserNameAsync(GetClaimedUserName());

            var result = await _photoService.AddPhotoAsync(formFile);

            if(result.Error is not null)
                return null;

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                AppUserId = user.Id
            };

            // if(user.Photos.Count == 0)
            //     photo.IsMain = true;

            user.Photos.Add(photo);

            return _mapper.Map<PhotoDto>(photo);
        }

        public async Task<bool> DeletePhoto(int photoId)
        {
            var user = await GetUserByUserNameAsync(GetClaimedUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if(photo is null) return false;
            if(photo.IsMain) return false;

            if(photo.PublicId is not null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if(result.Error is not null) return false;
            }

            user.Photos.Remove(photo);
            return true;
        }

        public async Task<MemberDto> GetMemberByIdAsync(int id)
        {
            return await _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<MemberDto> GetMemberByUserNameAsync(string username)
        {
            return await _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .Where(u => u.Username == username)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            userParams.CurrentUserName = GetClaimedUserName();
            var userGender = await GetUserGender(userParams.CurrentUserName);

            if(userGender is null)
                userGender = Male;

            if(string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = (userGender.ToLower() == Male) ? Female : Male;

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            var query = _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .Where(u => u.Username != userParams.CurrentUserName 
                    && u.Gender == userParams.Gender
                    && u.DateOfBirth >= minDob
                    && u.DateOfBirth <= maxDob)
                .OrderByDescending(u => userParams.OrderBy == "created" ? u.Created : u.LastActive)
                .AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<UnmoderatedPhotoDto>> GetUnmoderatedPhotos()
        {
            return await _context.Users
                .Include(u => u.Photos)
                .SelectMany(u => u.Photos)
                .Where(p => !p.IsModerated)
                .ProjectTo<UnmoderatedPhotoDto>(_mapper.ConfigurationProvider)
                .OrderBy(p => p.UserName)
                .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Photos.Where(p => p.IsModerated || u.UserName == GetClaimedUserName()))
                .SingleOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                .Where(u => u.UserName == username)
                .Select(u => u.Gender)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Photos.Where(p => p.IsModerated || u.UserName == GetClaimedUserName()))
                .ToListAsync();
        }

        public async Task<bool> SetMainPhotoAsync(int photoId)
        {
            var user = await GetUserByUserNameAsync(GetClaimedUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if(photo is null) return false;
            if(photo.IsMain) return false;

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

            if(currentMain is not null) currentMain.IsMain = false;

            photo.IsMain = true;
            
            return true;
        }

        public async Task<bool> SetModerated(int photoId)
        {
            var photo =  await _context.Users
                .Include(u => u.Photos)
                .SelectMany(u => u.Photos)
                .Where(p => !p.IsModerated)
                .SingleOrDefaultAsync(p => p.Id == photoId);


            if(photo is null) return false;

            photo.IsModerated = true;

            var userId = photo.AppUserId;
            
            var numberOfModeratedPhotos 
                =await  _context.Users
                    .Where(u => u.Id == userId)
                    .Include(u => u.Photos)
                    .SelectMany(u => u.Photos)
                    .Where(p => p.IsModerated)
                    .CountAsync();

            if(numberOfModeratedPhotos == 1) photo.IsMain = true;

            return true;
        }

        // public void Update(AppUser user)
        // {
        //     _context.Entry(user).State = EntityState.Modified;
        //     //_context.Update(user);
        // }

        public async Task UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var user = await GetUserByUserNameAsync(GetClaimedUserName());

            _mapper.Map(memberUpdateDto, user);
            _context.Update(user);
        }

        private string GetClaimedUserName()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}