using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var result = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(result.PaginationProperties);
            return Ok(result);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _unitOfWork.UserRepository.GetMemberByUserNameAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            await _unitOfWork.UserRepository.UpdateMember(memberUpdateDto);
            return await _unitOfWork.Complete() ? NoContent() : BadRequest("Failed to update user.");

            /*
            var username = User.FindFirst(.NameIdentifier)?.Value;
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.UserRepository.Update(user);

            if(await _unitOfWork.UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user.");
            */
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var result = await _unitOfWork.UserRepository.AddPhoto(file);

            if(_unitOfWork.HasChanges())
                await _unitOfWork.Complete();

            if (result is null)
                return BadRequest();

            return CreatedAtRoute("GetUser", new { username = User.GetUserName() }, result);
            //return CreatedAtRoute("GetUser", result);
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var success = await _unitOfWork.UserRepository.SetMainPhotoAsync(photoId);

            if(_unitOfWork.HasChanges())
                await _unitOfWork.Complete();

            return success? NoContent() : BadRequest("Failed to set main photo.");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var success = await _unitOfWork.UserRepository.DeletePhoto(photoId);

            if(_unitOfWork.HasChanges())
                await _unitOfWork.Complete();

            return success ? Ok() : BadRequest("Failed to delete photo.");
        }
    }
}