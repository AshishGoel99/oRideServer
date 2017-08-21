using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using oServer.UserModels;

namespace oServer.Controllers
{
    [Route("api/[controller]")]
    public class RidesController : Controller
    {
        DataAccess _access;
        public RidesController(DataAccess access)
        {
            _access = access;
        }
        // GET api/values/5
        [HttpGet("{latlng}")]
        public string Get(string latlng)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]UserModels.Ride ride)
        {
            var rideObj = getRideObj(ride);
            _access.CreateItem(rideObj);
            return new OkObjectResult(ride);
        }

        private DbModels.Ride getRideObj(Ride ride)
        {
            //for now set first User ID;
            var userId = _access.GetAll<DbModels.User>().First().Id;

            string fromId = null;
            string toId = null;
            if (ride.From != null)
            {
                var from = _access.GetAll<DbModels.Location>()
                    .Where(l => l.Name == ride.From.Name
                        || (l.Latitude == ride.From.Latitude && l.Longitude == ride.From.Longitude));

                if (from.Any())
                    fromId = from.First().Id.ToString();
                else
                {
                    var fromLoc = _access.CreateItem(new DbModels.Location
                    {
                        Name = ride.From.Name,
                        Longitude = ride.From.Longitude,
                        Latitude = ride.From.Latitude
                    });
                    fromId = fromLoc.Id.ToString();
                }
            }
            if (ride.To != null)
            {
                var to = _access.GetAll<DbModels.Location>()
                    .Where(l => l.Name == ride.To.Name
                        || (l.Latitude == ride.To.Latitude && l.Longitude == ride.To.Longitude));

                if (to.Any())
                    toId = to.First().Id.ToString();
                else
                {
                    var toLoc = _access.CreateItem(new DbModels.Location
                    {
                        Name = ride.To.Name,
                        Longitude = ride.To.Longitude,
                        Latitude = ride.To.Latitude
                    });
                    toId = toLoc.Id.ToString();
                }
            }

            return new DbModels.Ride
            {
                Duration = ride.Duration,
                Fare = ride.Fare,
                OwnerId = userId.ToString(),
                FromId = fromId,
                ToId = toId,
                StartTime = DateTime.ParseExact(ride.StartTime, "HH:mm", null),
                Note = ride.Note,
                Vehicle = ride.Vehicle
            };
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
