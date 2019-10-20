using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var current_userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repository.GetUser(current_userId);
            userParams.UserId = current_userId;
            if (String.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
            var users = await _repository.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            Response.AddPagination(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(usersToReturn);
        }

        [HttpGet("{user_id}",Name = "GetUser")]
        public async Task<IActionResult> GetUser(int user_id)
        {
            var user = await _repository.GetUser(user_id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{user_id}")]
        public async Task<IActionResult> UpdateUser(int user_id, UserForUpdateDto userForUpdateDto)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _repository.GetUser(user_id);
            _mapper.Map(userForUpdateDto, userFromRepo);
            if (await _repository.SaveAll())
            {
                return NoContent();
            }
            
            throw new Exception($"some thing is going wrong with {user_id}");
        }

        [HttpPost("{user_id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int user_id, int recipientId)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _repository.GetLike(user_id, recipientId);
            if (like != null)
            {
                return BadRequest("You already like this user.");
            }

            if (await _repository.GetUser(recipientId) == null)
            {
                return NotFound();
            }
            
            like = new Like()
            {
                LikerId = user_id,
                LikeeId = recipientId
            };
            
            _repository.Add<Like>(like);

            if (await _repository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like user");
        }
    }
}