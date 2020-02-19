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
using System.Collections.Generic;
using System.Linq;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
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

            var user = await GetUser(Credentials.Email);

            if (user == null)
            {
                if (string.IsNullOrWhiteSpace(Credentials.FbToken))
                    return BadRequest();

                //also needs to validate access token here.
                var id = Guid.NewGuid().ToString();
                user = new User
                {
                    Id = id,
                    FirstName = Credentials.FirstName,
                    Email = Credentials.Email,
                    FbId = Credentials.FbId,
                    Picture = Credentials.Picture,
                    UserName = Credentials.UserName,
                    PushId = Credentials.PushId
                };

                if (!await Register(Credentials, id))
                {
                    return StatusCode(500);
                }
            }

            var userData = GetUserData(user.Id);

            var claims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                    };

            var token = GenerateToken(claims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                data = await userData
            });
        }

        private async Task<UserData> GetUserData(string id)
        {
            UserData data = new UserData();
            await MySqlDataAccess.Instance.Get(
                "Select " +
                    "rides.Id, firstname, GoTime, ReturnTime, `from`, ST_AsText(fromlatlng), `to`, ST_AsText(tolatlng), note, PolyLine, " +
                    "ScheduleType, Days, `Date`, SeatsAvail, Price, VehicleNo, ContactNo, ST_AsText(Way1LatLng), ST_AsText(Way2LatLng), " +
                    "ST_AsText(Way3LatLng), Bounds, ST_AsText(Polygon) " +
                "from rides join users on rides.userid=users.id where users.id=@p1",
                                parameters: id, readFromReader: async (DbDataReader reader) =>
                                {
                                    data.Rides.Add(new Ride
                                    {
                                        Id = await reader.GetValueFromIndex<string>(0),
                                        Owner = await reader.GetValueFromIndex<string>(1),
                                        StartTime = (await reader.GetValueFromIndex<TimeSpan>(2)).ToString(),
                                        ReturnTime = (await reader.GetValueFromIndex<TimeSpan>(3)).ToString(),
                                        From = new Location
                                        {
                                            Name = await reader.GetValueFromIndex<string>(4),
                                            LatLng = await reader.GetValueFromIndex<string>(5),
                                        },
                                        To = new Location
                                        {
                                            Name = await reader.GetValueFromIndex<string>(6),
                                            LatLng = await reader.GetValueFromIndex<string>(7),
                                        },
                                        Note = await reader.GetValueFromIndex<string>(8),
                                        PolyLine = await reader.GetValueFromIndex<string>(9),
                                        ScheduleType = await reader.GetValueFromIndex<short>(10),
                                        Days = (await reader.GetValueFromIndex<string>(11)).Split(',')
                                            .Select(c => { return short.Parse(c); }).ToList(),
                                        Date = await reader.GetValueFromIndex<string>(12),
                                        SeatsAvail = await reader.GetValueFromIndex<short>(13),
                                        Fare = await reader.GetValueFromIndex<float>(14),
                                        Vehicle = await reader.GetValueFromIndex<string>(15),
                                        ContactNo = await reader.GetValueFromIndex<string>(16),
                                        Waypoints = new List<string>{
                                                    await reader.GetValueFromIndex<string>(17),
                                                    await reader.GetValueFromIndex<string>(18),
                                                    await reader.GetValueFromIndex<string>(19)
                                    },
                                        Bounds = await reader.GetValueFromIndex<string>(20),
                                        PolyGon = await reader.GetValueFromIndex<string>(21)
                                    });
                                });
            return data;
        }

        private async Task<User> GetUser(string email)
        {
            User user = null;

            await MySqlDataAccess.Instance.Get("Select Id, FirstName, Email, UserName, FbId, Picture, PushId from users where email=@p1",
                                parameters: email, readFromReader: async (DbDataReader reader) =>
                                {
                                    user = new User()
                                    {
                                        Id = await reader.GetValueFromIndex<string>(0),
                                        FirstName = await reader.GetValueFromIndex<string>(1),
                                        Email = await reader.GetValueFromIndex<string>(2),
                                        UserName = await reader.GetValueFromIndex<string>(3),
                                        FbId = await reader.GetValueFromIndex<string>(4),
                                        Picture = await reader.GetValueFromIndex<string>(5),
                                        PushId = await reader.GetValueFromIndex<string>(6)
                                    };
                                });
            return user;
        }

        private async Task<bool> Register(Credentials model, string id)
        {
            var result = await MySqlDataAccess.Instance
                .Execute("insert into users values(@p1,@p2,@p3,@p4,@p5,@p6,@p7)",
                    id, model.FirstName, model.Email, model.UserName, model.FbId, model.Picture, model.PushId);

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