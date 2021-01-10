using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> LikeUser(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikesAsync(sourceUserId);

            if (sourceUser is null) return NotFound();
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself!");

            var userlike = await _unitOfWork.LikesRepository.GetUserLikeAsync(sourceUserId, likedUser.Id);

            if (userlike is not null) return BadRequest("You already like this user!");

            sourceUser.LikedUsers.Add(new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            });

            return await _unitOfWork.Complete() ? Ok() : BadRequest();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLike([FromQuery] LikesParams likedParams)
        {
            likedParams.UserId = User.GetUserId();

            var result = await _unitOfWork.LikesRepository.GetUserLikesAsync(likedParams);

            Response.AddPaginationHeader(result.PaginationProperties);

            return Ok(await _unitOfWork.LikesRepository.GetUserLikesAsync(likedParams));
        }
    }
}