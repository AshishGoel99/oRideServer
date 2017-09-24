using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using oServer.UserModels;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecificOrigin")]
    public class RidesController : Controller
    {
        public RidesController()
        {
        }
        // GET api/values/5
        // [HttpGet]
        public async Task<IActionResult> Get([FromQuery]SearchQuery query)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var date = DateTime.Parse(query.Time);
            var timeOfDay = date.TimeOfDay.Subtract(new TimeSpan(1, 0, 0)); //to check if someone left 1 hour earlier
            var timeBuffer = timeOfDay.Add(new TimeSpan(query.Frame + 1, 0, 0));
            var rides = new List<UserModels.Ride>();

            var q = "select " +
                        "rides.Id, firstname, GoTime, ReturnTime, `from`, ST_AsText(fromlatlng), `to`, ST_AsText(tolatlng), note, PolyLine, " +
                        "ScheduleType, Days, `Date`, SeatsAvail, Price, VehicleNo, ContactNo, ST_AsText(Way1LatLng), ST_AsText(Way2LatLng), " +
                        "ST_AsText(Way3LatLng), Bounds, ST_AsText(Polygon) " +
                    "from rides " +
                    "join users on rides.userid=users.id " +
                    "where " +
                        "SeatsAvail>0" + //Seats available
                        " and ((scheduleType=0 and days like @p3) or (scheduleType=1 and date = @p4))" +//Schedule check
                        " and ST_CONTAINS(polygon, GeomFromText(@p1)) and ST_CONTAINS(polygon, GeomFromText(@p2))" + //region check
                        " and ((ST_Distance(GeomFromText(@p1),FromLatLng) < ST_Distance(GeomFromText(@p2),ToLatLng)" + //in Go Direction
                                " and GoTime >=@p5 and GoTime<=@p6)" + // and also go time
                            " or " +
                            "(ST_Distance(GeomFromText(@p1),ToLatLng) < ST_Distance(GeomFromText(@p2),FromLatLng)" + //in Return Direction
                                " and ReturnTime >=@p5 and ReturnTime<=@p6))"; // and also return time

            await MySqlDataAccess.Instance.Get(q,
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
                                    PolyGon = await reader.GetValueFromIndex<string>(21)
                                });
                            },
                            query.From, query.To, "%" + ((int)date.DayOfWeek - 1) + "%", date, timeOfDay, timeBuffer);

            return Ok(rides);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserModels.Ride ride)
        {
            var uId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            while (ride.Waypoints.Count < 3)
                ride.Waypoints.Add(null);

            var result = await MySqlDataAccess.Instance.Execute(
                "INSERT INTO oride.rides(Id, PolyLine, Bounds, Polygon, GoTime, ReturnTime, ScheduleType, Date," +
                "Days, SeatsAvail, Price, ContactNo, FromLatLng, ToLatLng, Way1LatLng, Way2LatLng, Way3LatLng, `From`," +
                "`To`, UserId, VehicleNo) VALUES(@p1, @p2, @p3, GeomFromText(@p4), @p5, @p6, @p7, @p8, @p9, @p10, @p11," +
                "@p12, GeomFromText(@p13), GeomFromText(@p14), GeomFromText(@p15), GeomFromText(@p16), GeomFromText(@p17)," +
                "@p18, @p19, @p20, @p21)",
                Guid.NewGuid(), ride.PolyLine, ride.Bounds, ride.PolyGon, ride.StartTime, ride.ReturnTime,
                ride.ScheduleType, ride.Date, string.Join(',', ride.Days), ride.SeatsAvail, ride.Fare, ride.ContactNo, ride.From.LatLng,
                ride.To.LatLng, ride.Waypoints[0], ride.Waypoints[1], ride.Waypoints[2], ride.From.Name, ride.To.Name,
                uId, ride.Vehicle);

            return result == 1 ? Ok() : StatusCode(500);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
