using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace EventGenerator
{
    public class SeatEventPublisher
    {
        private readonly CloudQueueClient _queueClient;
        private readonly CloudQueue _queue;
     
        public SeatEventPublisher(string connectionString, string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            _queueClient = storageAccount.CreateCloudQueueClient();
            _queue = _queueClient.GetQueueReference(queueName);
            _queue.CreateIfNotExists();
        }

        public Task PublishEventsAsync(Guid roomId, int seatNumber, bool isTaken)
        {
            var @event = new SeatEvent
            {
                RoomId = roomId,
                SeatNumber = seatNumber,
                IsTaken = isTaken
            };

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(@event));

            return _queue.AddMessageAsync(message);
        }
    }
}