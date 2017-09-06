using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using oServer.DbModels;
using oServer.UserModels;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    public class RidesController : Controller
    {
        public RidesController()
        {
        }
        // GET api/values/5
        [HttpGet("{lat}/{lng}")]
        public IEnumerable<UserModels.Ride> Get(SearchQuery query)
        {
            var timeOfDay = DateTime.Now.TimeOfDay.Subtract(new TimeSpan(1, 0, 0));
            var timeBuffer = timeOfDay.Add(new TimeSpan(query.TimeFrame + 1, 0, 0));

            var result = MySqlDataAccess.Instance.Get("select * from rides where " +
                            "SeatsAvail>0 and " + //Seats available
                            "((scheduleType=0 and days like '%@p3%') or (scheduleType=1 and date = @p4) )" +//Schedule check
                            "ST_CONTAINS(polygon, GeomFromText(@p1) and ST_CONTAINS(polygon, GeomFromText(@p2)" + //region check
                            "((ST_Distance(GeomFromText(@p1),FromLatLng) < ST_Distance(GeomFromText(@p2),ToLatLng) " + //in Go Direction
                            "and GoTime >=@p5 and GoTime<=@p6) " + // and also go time
                            "or " +
                            "(ST_Distance(GeomFromText(@p1),FromLatLng) < ST_Distance(GeomFromText(@p2),ToLatLng) " + //in Return Direction
                            "and ReturnTime >=@p5 and ReturnTime<=@p6))" + // and also return time
                            query.From.LatLng, query.To.LatLng, (int)DateTime.Today.DayOfWeek, DateTime.Now.Date, timeOfDay, timeBuffer);

            var rides = new List<UserModels.Ride>();

            foreach (DataRow item in result.Tables[0].Rows)
            {
                rides.Add(new UserModels.Ride
                {
                    Date = Convert.ToString(item["date"]),
                    ContactNo = Convert.ToString(item["ContactNo"])
                });
            }

            // var rides = DataAccess.Instance.GetAll<DbModels.Ride>
            //             (queryRides(query))
            //             .Where(r => { return matchedRides(r, query); })
            //             .Select(r => GenerateUserModel(r))
            //             .Take(5);
            // return rides;
            return rides;
        }

        // private IMongoQuery queryRides(SearchQuery query)
        // {
        //     double[,] points = { { query.Longitude }, { query.Latitude } };


        //     var finalQuery = Query.Empty;

        //     var withinPolyGon = Query<DbModels.Ride>.WithinPolygon(r => r.PolyGon, points);
        //     var seatsAvailablity = Query<DbModels.Ride>.GT(r => r.SeatsAvail, 0);
        //     var scheduleType = Query.Or(
        //         Query.And(Query<DbModels.Ride>.EQ(r => r.ScheduleType, 0), //If weekly then match day
        //             Query<DbModels.Ride>.In(r => r.Days, new short[] { (short)DateTime.Now.DayOfWeek })),
        //         Query.And(Query<DbModels.Ride>.EQ(r => r.ScheduleType, 1), //If Specific day then match
        //             Query<DbModels.Ride>.EQ(r => r.Date, DateTime.Now.Date)));

        //     var timeFrame = Query.Or(
        //         Query.And(
        //             Query<DbModels.Ride>.GTE(r => r.StartTime, DateTime.Now), //if going time matches
        //             Query<DbModels.Ride>.LTE(r => r.StartTime, DateTime.Now.AddHours(query.TimeFrame))),
        //         Query.And(
        //             Query<DbModels.Ride>.GTE(r => r.ReturnTime, DateTime.Now), //if return time matches
        //             Query<DbModels.Ride>.LTE(r => r.ReturnTime, DateTime.Now.AddHours(query.TimeFrame))));

        //     //Filter the direction .Net from result
        //     //Also Needs to match common route if different

        //     finalQuery = Query.And(seatsAvailablity,
        //         scheduleType,
        //         withinPolyGon);

        //     return finalQuery;
        // }

        // private bool matchedRides(DbModels.Ride r, SearchQuery query)
        // {
        //     // if (r.)

        //     return false;
        // }

        // private UserModels.Ride GenerateUserModel(DbModels.Ride r)
        // {
        //     throw new NotImplementedException();
        // }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]UserModels.Ride ride)
        {
            // var rideObj = GenerateDbModel(ride);
            // DataAccess.Instance.CreateItem(rideObj);
            // return new OkObjectResult(ride);

            while (ride.Waypoints.Length < 3)
                ride.Waypoints = (string[])ride.Waypoints.Append(string.Empty);

            var result = MySqlDataAccess.Instance.Execute(
                "INSERT INTO oride.rides(Id, PolyLine, Bounds, Polygon, GoTime, ReturnTime, ScheduleType, Date," +
                "Days, SeatsAvail, Price, ContactNo, FromLatLng, ToLatLng, Way1LatLng, Way2LatLng, Way3LatLng, From," +
                "To, UserId, VehicleNo) VALUES(@p1, @p2, @p3, GeomFromText(@p4), @p5, @p6, @p7, @p8, @p9, @p10, @p11," +
                "@p12, GeomFromText(@p13), GeomFromText(@p14), @p15, @p16, @p17, @18, @p19, @p20, @p21, @p22, @p23, @p24)",
                Guid.NewGuid(), ride.PolyLine, ride.Bounds, ride.PolyGon, ride.StartTime, ride.ReturnTime,
                ride.ScheduleType, ride.Date, ride.Days, ride.SeatsAvail, ride.Fare, ride.ContactNo, ride.From.LatLng, ride.To.LatLng, ride.Waypoints[0], ride.Waypoints[1], ride.Waypoints[2], ride.From.Name, ride.To.Name, User.Identity.Name, ride.Vehicle);

            return result == 1 ? Ok() : StatusCode(500);
        }

        // private DbModels.Ride GenerateDbModel(UserModels.Ride ride)
        // {
        //     //for now set first User ID;
        //     var userId = DataAccess.Instance.GetAll<DbModels.User>().First().Id;

        //     string fromId = null;
        //     string toId = null;
        //     if (ride.From != null)
        //     {
        //         var from = DataAccess.Instance.GetAll<DbModels.Location>()
        //             .Where(l => l.Name == ride.From.Name
        //                 || (l.Latitude == ride.From.Latitude && l.Longitude == ride.From.Longitude));

        //         if (from.Any())
        //             fromId = from.First().Id.ToString();
        //         else
        //         {
        //             var fromLoc = DataAccess.Instance.CreateItem(new DbModels.Location
        //             {
        //                 Name = ride.From.Name,
        //                 Longitude = ride.From.Longitude,
        //                 Latitude = ride.From.Latitude
        //             });
        //             fromId = fromLoc.Id.ToString();
        //         }
        //     }
        //     if (ride.To != null)
        //     {
        //         var to = DataAccess.Instance.GetAll<DbModels.Location>()
        //             .Where(l => l.Name == ride.To.Name
        //                 || (l.Latitude == ride.To.Latitude && l.Longitude == ride.To.Longitude));

        //         if (to.Any())
        //             toId = to.First().Id.ToString();
        //         else
        //         {
        //             var toLoc = DataAccess.Instance.CreateItem(new DbModels.Location
        //             {
        //                 Name = ride.To.Name,
        //                 Longitude = ride.To.Longitude,
        //                 Latitude = ride.To.Latitude
        //             });
        //             toId = toLoc.Id.ToString();
        //         }
        //     }

        //     return new DbModels.Ride
        //     {
        //         // Duration = ride.Duration,
        //         Fare = ride.Fare,
        //         OwnerId = userId.ToString(),
        //         FromId = fromId,
        //         ToId = toId,
        //         StartTime = DateTime.ParseExact(ride.StartTime, "HH:mm", null),
        //         Note = ride.Note,
        //         Vehicle = ride.Vehicle
        //     };
        // }

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
