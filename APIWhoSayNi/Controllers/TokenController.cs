using API.Commons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APIWhoSayNi.Controllers
{
    [Route("[controller]")]
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;

        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RequestToken([FromBody] string password, CancellationToken cancellationToken)
        {
            if (await PasswordIsValid(password))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.SerialNumber, password)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CommonConstants.SecurityKey));
                var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: "APIWhoSayNi",
                    audience: "APIWhoSayNi",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credential);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return BadRequest("Invalid request");
        }

        private async Task<bool> PasswordIsValid(string password)
        {
            // Some code to audit
            return true;
        }
    }
}