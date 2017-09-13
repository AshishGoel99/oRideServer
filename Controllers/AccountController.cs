using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System;
using oServer.UserModels;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using oServer.DbModels;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.AspNetCore.Cors;

namespace oServer.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly JWTSettings _options;

        public AccountController(
          IOptions<JWTSettings> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SignIn([FromBody] Credentials Credentials)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Input");

            var user = GetUser(Credentials.Email);

            if (user == null)
            {
                if (string.IsNullOrWhiteSpace(Credentials.AccessToken))
                    return BadRequest();

                //also needs to validate access token here.

                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Credentials.Name,
                    Email = Credentials.Email
                };

                if (!await Register(Credentials))
                {
                    return StatusCode(500);
                }
            }

            var claims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.GivenName, user.Name),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                    };

            var token = GenerateToken(claims);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        private User GetUser(string email)
        {
            User user = null;

            MySqlDataAccess.Instance.Get("Select * from users where email=@p1",
                                parameters: email, readFromReader: async (DbDataReader reader) =>
                                {
                                    user = new User()
                                    {
                                        Id = await reader.GetFieldValueAsync<string>(0),
                                        Name = await reader.GetFieldValueAsync<string>(1),
                                        Email = await reader.GetFieldValueAsync<string>(2)
                                    };
                                });
            return user;
        }

        private async Task<bool> Register(Credentials model)
        {
            var result = await MySqlDataAccess.Instance
                .Execute("insert into users values(@p1,@p2,@p3)", Guid.NewGuid(), model.Name, model.Email);

            return result == 1;
        }

        private JwtSecurityToken GenerateToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(_options.Issuer,
              _options.Audience,
              claims,
              expires: DateTime.Now.AddHours(24),
              signingCredentials: creds);
        }
    }
}