using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace oServer.UserModels
{
    public class UserData
    {
        public List<Ride> Rides { get; set; }
    }
}