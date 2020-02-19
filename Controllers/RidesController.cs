using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using oServer.DbModels;
using oServer.UserModels;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecificOrigin")]
    public class RidesController : Controller
    {
        private string uId;
        private const string FCM_SERVER_KEY = "AAAAUifbFe4:APA91bE4Ku-hSvUMBP1HUIwVQoV-ucfIJ8WUxQa4QX0UR09TQ798aG58Rx_Ru3yxV7VztSjEaZuZamjCisuCjLa4T4SXklQjl5RbnHL4ORwGYeGKQUtw9Hin5olFgkP1fwJyR2wdCpNm";
        private const string FCM_SENDER_ID = "352855987694";

        public RidesController()
        {
            uId = this.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        [Route("SeatRequest")]
        public async Task<IActionResult> SeatRequest([FromBody]String routeId, [FromBody]String count)
        {
            var result = await MySqlDataAccess.Instance.Execute(
                            "insert into seatrequest(routeId, userId, requestedNumber) values(@p1, @p2, @p3)", routeId, uId, count);

            if (result != 1)
                return StatusCode(500);

            sendPushToRouteOwner(routeId);
            return Ok();
        }

        private async void sendPushToRouteOwner(string routeId)
        {
            var userInfo = await getUserInfoFromRouteId(routeId);
            sendPush(userInfo.FirstName + " is requesting for a seat.", userInfo.PushId);
        }

        private void sendPush(string message, string regId)
        {
            var tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", FCM_SERVER_KEY));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", FCM_SENDER_ID));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                to = regId,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = "oRide",
                    title = message,
                    badge = 1
                },
            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (var dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (var dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null)
                            using (var tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                //result.Response = sResponseFromServer;
                            }
                    }
                }
            }
        }

        private async Task<User> getUserInfoFromRouteId(string routeId)
        {
            User userInfo = null;
            await MySqlDataAccess.Instance.Get(
                "Select  FirstName, PushId" +
                " from rides join users on rides.userid=users.id where rides.id=@p1",
                                parameters: routeId, readFromReader: async (DbDataReader reader) =>
                                {
                                    userInfo = new User()
                                    {
                                        FirstName = await reader.GetValueFromIndex<string>(0),
                                        PushId = await reader.GetValueFromIndex<string>(1)
                                    };
                                });
            return userInfo;
        }

        // GET api/values/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery]SearchQuery query)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var date = DateTime.Parse(query.Time);
            TimeSpan timeOfDay = TimeSpan.Zero, timeBuffer = TimeSpan.Zero;

            if (query.Frame != null)
            {
                timeOfDay = date.TimeOfDay;
                timeBuffer = timeOfDay.Add(new TimeSpan((int)query.Frame, 0, 0));
            }
            var rides = new List<UserModels.Ride>();

            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("fromLatLng", query.From));
            parameters.Add(new MySqlParameter("toLatLng", query.To));
            parameters.Add(new MySqlParameter("dayofWeek", "%" + ((int)date.DayOfWeek - 1) + "%"));
            parameters.Add(new MySqlParameter("dateOfJourney", date));
            parameters.Add(new MySqlParameter("userId", uId));

            var sp = StoredProcedures.GetTomorrowRides;
            if (query.Frame != null)
            {
                sp = StoredProcedures.GetTodayRides;
                parameters.Add(new MySqlParameter("timeOfDay", timeOfDay));
                parameters.Add(new MySqlParameter("timeBuffer", timeBuffer));
            }


            await MySqlDataAccess.Instance.Get(sp,
                            async (DbDataReader reader) =>
                            {
                                rides.Add(new UserModels.Ride
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
                                    PolyGon = await reader.GetValueFromIndex<string>(21),
                                    Active = await reader.GetValueFromIndex<UInt64>(22) == 1,
                                });
                            },
                            parameters.ToArray());

            return Ok(rides);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserModels.Ride ride)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            while (ride.Waypoints.Count < 3)
                ride.Waypoints.Add(null);

            var id = Guid.NewGuid().ToString();

            var result = await MySqlDataAccess.Instance.Execute(
                "INSERT INTO oride.rides(Id, PolyLine, Bounds, Polygon, GoTime, ReturnTime, ScheduleType, Date," +
                "Days, SeatsAvail, Price, ContactNo, FromLatLng, ToLatLng, Way1LatLng, Way2LatLng, Way3LatLng, `From`," +
                "`To`, UserId, VehicleNo) VALUES(@p1, @p2, @p3, ST_GeomFromText(@p4), @p5, @p6, @p7, @p8, @p9, @p10, @p11," +
                "@p12, ST_GeomFromText(@p13), ST_GeomFromText(@p14), ST_GeomFromText(@p15), ST_GeomFromText(@p16), ST_GeomFromText(@p17)," +
                "@p18, @p19, @p20, @p21)",
                id, ride.PolyLine, ride.Bounds, ride.PolyGon, ride.StartTime, ride.ReturnTime,
                ride.ScheduleType, ride.Date, string.Join(',', ride.Days), ride.SeatsAvail, ride.Fare, ride.ContactNo, ride.From.LatLng,
                ride.To.LatLng, ride.Waypoints[0], ride.Waypoints[1], ride.Waypoints[2], ride.From.Name, ride.To.Name,
                uId, ride.Vehicle);

            if (result == 1)
                return Ok(id);

            return StatusCode(500);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody]UserModels.Ride ride)
        {
            var result = await MySqlDataAccess.Instance.Execute(
                "UPDATE oride.rides " +
                "set GoTime=@p1, ReturnTime=@p2, Date=@p3, Days=@p4, SeatsAvail=@p5, Price=@p6, Active=@p9 " +
                "WHERE id=@p7 and userid=@p8",
                ride.StartTime, ride.ReturnTime, ride.Date, string.Join(',', ride.Days), ride.SeatsAvail, ride.Fare, ride.Id, uId, ride.Active ? 1 : 0);

            if (result == 1)
                return Ok();

            return StatusCode(500);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await MySqlDataAccess.Instance.Execute(
                "DELETE from oride.rides " +
                "WHERE id=@p1 and userid=@p2", id, uId);

            if (result == 1)
                return Ok();

            return StatusCode(500);
        }
    }
}
