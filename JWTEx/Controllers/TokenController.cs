using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JWTEx.Controllers
{
    [Produces("application/json")]
    [Route("api/Token")]
    public class TokenController : Controller
    {
        private IConfiguration _config;

        public TokenController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]AccessToken access)
        {
            IActionResult response = Unauthorized();
            var user = Authenticate(access);

            if (user != null)
            {
                var tokenString = BuildToken(access);

                var datepd = DateTime.UtcNow.AddMinutes(45); //Expired Token  

                user.password = access.password;
                user.token = tokenString.ToString();
                user.dateexpd = datepd;

                response = Ok(new
                {
                    token = tokenString,
                    username = user.username,
                    expiresin = datepd
                });
            }

            return response;
        }

        private string BuildToken(AccessToken access)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              expires: DateTime.UtcNow.AddHours(7).AddYears(3),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private AccessToken Authenticate(AccessToken access)
        {
            AccessToken data = new AccessToken();
            if (access.username == "camellabs" && access.password == "camellabs")
            {
                data = access;
            }
            else
            {
                data = null;
            }

            return data;
        }
    }
}
