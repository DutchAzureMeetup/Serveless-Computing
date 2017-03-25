using System;

namespace EventGenerator
{
    public class SeatEvent
    {
        public Guid RoomId { get; set; }

        public int SeatNumber { get; set; }

        public bool IsTaken { get; set; }
    }
}
