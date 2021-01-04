using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        //void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<bool> UpdateMemberAsync(MemberUpdateDto memberUpdateDto);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUserNameAsync(string username);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        Task<MemberDto> GetMemberByIdAsync(int id);
        Task<MemberDto> GetMemberByUserNameAsync(string username);
        Task<PhotoDto> AddPhoto(IFormFile formFile);
        Task<bool> SetMainPhotoAsync(int photoId);
        Task<bool> DeletePhoto(int photoId);
    }
}