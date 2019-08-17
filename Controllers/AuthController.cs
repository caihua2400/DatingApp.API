using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
             userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();
            if (await _authRepository.UserExist(userForRegisterDto.UserName))
            {
                return BadRequest("username already exists.");
            }

            var userToCreate = new User()
            {
                UserName = userForRegisterDto.UserName
            };

            var createdUser = _authRepository.Register(userToCreate,userForRegisterDto.Password);

            return StatusCode(201);

        }
        
        
    }
}