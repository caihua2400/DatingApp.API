using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration, IMapper mapper)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register( UserForRegisterDto userForRegisterDto)
        {


            
            userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();
            if (await _authRepository.UserExist(userForRegisterDto.UserName))
            {
                return BadRequest("username already exists.");
            }

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser =await _authRepository.Register(userToCreate,userForRegisterDto.Password);
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            return CreatedAtRoute("GetUser",  new
            {
                controller= "Users",
                user_id=createdUser.Id
            },userToReturn);

        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn(UserForLoginDto userForLoginDto)
        {
              
            
                var userFromRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
                if (userFromRepo == null)
                {
                    return Unauthorized();
                }

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.UserName)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.
                    GetBytes(_configuration.GetSection("AppSettings:Token").Value));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                var user = _mapper.Map<UserForListDto>(userFromRepo);
                return Ok(new
                {
                    token = tokenHandler.WriteToken(token),
                    user
                });
           
           
        }
        
        
    }
}