using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oServer.UserModels;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RidesController : Controller
    {
        public RidesController()
        {
        }
        // GET api/values/5
        // [HttpGet]
        public IEnumerable<UserModels.Ride> Get([FromQuery]SearchQuery query)
        {
            // var u = User.Ide.FindAll(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
                return null;

            var timeOfDay = DateTime.Now.TimeOfDay.Subtract(new TimeSpan(1, 0, 0));
            var timeBuffer = timeOfDay.Add(new TimeSpan(query.Frame + 1, 0, 0));
            var rides = new List<UserModels.Ride>();

            var q = "select * from rides where " +
                            "SeatsAvail>0 and " + //Seats available
                            "((scheduleType=0 and days like '%@p3%') or (scheduleType=1 and date = @p4) )" +//Schedule check
                            "ST_CONTAINS(polygon, GeomFromText(@p1) and ST_CONTAINS(polygon, GeomFromText(@p2)" + //region check
                            "((ST_Distance(GeomFromText(@p1),FromLatLng) < ST_Distance(GeomFromText(@p2),ToLatLng) " + //in Go Direction
                            "and GoTime >=@p5 and GoTime<=@p6) " + // and also go time
                            "or " +
                            "(ST_Distance(GeomFromText(@p1),FromLatLng) < ST_Distance(GeomFromText(@p2),ToLatLng) " + //in Return Direction
                            "and ReturnTime >=@p5 and ReturnTime<=@p6))"; // and also return time

            MySqlDataAccess.Instance.Get(q,
                            async (DbDataReader reader) =>
                            {
                                rides.Add(new UserModels.Ride
                                {
                                    Date = await reader.GetFieldValueAsync<string>(0), //Convert.ToString(item["date"]),
                                    ContactNo = await reader.GetFieldValueAsync<string>(1)//Convert.ToString(item["ContactNo"])
                                });
                            },
                            query.From.LatLng, query.To.LatLng, (int)DateTime.Today.DayOfWeek, DateTime.Now.Date, timeOfDay, timeBuffer);

            return rides;
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserModels.Ride ride)
        {
            while (ride.Waypoints.Length < 3)
                ride.Waypoints = (string[])ride.Waypoints.Append(string.Empty);

            var result = await MySqlDataAccess.Instance.Execute(
                "INSERT INTO oride.rides(Id, PolyLine, Bounds, Polygon, GoTime, ReturnTime, ScheduleType, Date," +
                "Days, SeatsAvail, Price, ContactNo, FromLatLng, ToLatLng, Way1LatLng, Way2LatLng, Way3LatLng, From," +
                "To, UserId, VehicleNo) VALUES(@p1, @p2, @p3, GeomFromText(@p4), @p5, @p6, @p7, @p8, @p9, @p10, @p11," +
                "@p12, GeomFromText(@p13), GeomFromText(@p14), @p15, @p16, @p17, @18, @p19, @p20, @p21, @p22, @p23, @p24)",
                Guid.NewGuid(), ride.PolyLine, ride.Bounds, ride.PolyGon, ride.StartTime, ride.ReturnTime,
                ride.ScheduleType, ride.Date, ride.Days, ride.SeatsAvail, ride.Fare, ride.ContactNo, ride.From.LatLng, 
                ride.To.LatLng, ride.Waypoints[0], ride.Waypoints[1], ride.Waypoints[2], ride.From.Name, ride.To.Name, 
                User.Identity.Name, ride.Vehicle);

            return result == 1 ? Ok() : StatusCode(500);
        }

        // // PUT api/values/5
        // [HttpPut("{id}")]
        // public void Put(int id, [FromBody]string value)
        // {
        // }

        // // DELETE api/values/5
        // [HttpDelete("{id}")]
        // public void Delete(int id)
        // {
        // }
    }
}
