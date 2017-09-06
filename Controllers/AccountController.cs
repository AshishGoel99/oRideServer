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

namespace oServer.Controllers
{
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
            if (ModelState.IsValid)
            {
                var user = GetUser(Credentials.Email);


                // var result = DataAccess.Instance.GetAll<User>()
                //     .Where(u => u.Email == Credentials.Email);

                // User user = null;
                if (user == null)
                {
                    if (Register(Credentials))
                    {
                        user = new User
                        {
                            Name = Credentials.Name,
                            Email = Credentials.Email
                        };
                    }
                }

                var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                var token = GenerateToken(claims);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return BadRequest("Invalid Input");
        }

        private User GetUser(string email)
        {
            var result = MySqlDataAccess.Instance
                                .Get("Select * from users where email=@p1", email);

            User user = null;
            if (result != null && result.Tables.Count > 0 && result.Tables[0] != null)
            {
                user = new User
                {
                    Email = email,
                    Name = Convert.ToString(result.Tables[0].Rows[0]["Name"])
                };
            }
            return user;
        }

        private bool Register(Credentials model)
        {
            var result = MySqlDataAccess.Instance
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
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);
        }
    }
}