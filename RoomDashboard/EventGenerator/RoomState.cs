using System;
using System.Collections.Generic;
using System.Linq;

namespace EventGenerator
{
    public class RoomState
    {
        private const int SeatsPerRoom = 100;

        public RoomState()
        {
            Action = RoomAction.Enter;
            TakenSeats = new List<int>();
            EmptySeats = Enumerable.Range(1, SeatsPerRoom).ToList();
        }

        public Guid RoomId { get; set; }

        public RoomAction Action { get; set; }

        public List<int> TakenSeats { get; set; }

        public List<int> EmptySeats { get; set; }

        public SeatEventPublisher EventPublisher { get; set; }
    }
}
