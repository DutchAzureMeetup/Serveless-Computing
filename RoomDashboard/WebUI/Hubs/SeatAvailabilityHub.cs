using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WebUI.Hubs
{
    public class SeatAvailabilityHub : Hub
    {
        public void Send(Guid roomId, int seatAvailability)
        {
            Clients.All.updateSeatAvailability(roomId, seatAvailability);
        }
    }
}