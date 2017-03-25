using System;

namespace Shared
{
    public class Room
    {
        public Guid RoomId { get; set; }

        public string RoomName { get; set; }

        public string QueueConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}