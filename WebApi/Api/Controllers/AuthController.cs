using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Services.Interfaces;

namespace WebApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;

        public AuthController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] TokenRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Role))
            {
                return BadRequest("Invalid request");
            }

            var token = _tokenService.GenerateToken(request.Username, request.Role, "abc");

            return Ok(new { Token = token });
        }
    }

    public class TokenRequestDto
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }
}