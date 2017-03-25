using System;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using WebUI.Hubs;

namespace WebUI.Controllers
{
    public class RoomController : ApiController
    {
        public void Put(Guid id, [FromBody]int availableSeats)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<SeatAvailabilityHub>();

            context.Clients.All.updateSeatAvailability(id, availableSeats);
        }
    }
}
