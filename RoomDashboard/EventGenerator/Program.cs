using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Shared;

namespace EventGenerator
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        private const int MaxTakeSeatCount = 20;
        private const int MaxLeaveSeatCount = 30;

        private static readonly TimeSpan DelayTime = TimeSpan.FromSeconds(3);
        private static readonly Random random = new Random();

        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            using (var host = new JobHost(config))
            {
                var token = new WebJobsShutdownWatcher().Token;
                var methodInfo = typeof(Program).GetMethod(nameof(RunAsync));
                host.CallAsync(methodInfo, token).GetAwaiter().GetResult();
            }
        }

        [NoAutomaticTrigger]
        public static async Task RunAsync(CancellationToken cancellationToken)
        {
            var connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            var roomRepository = new RoomRepository(connectionString);
            var roomStateRepository = new RoomStateRepository(roomRepository);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var publishTasks = (await roomStateRepository.GetRoomsAsync())
                    .Select(rs => PublishRoomEventsAsync(rs));

                await Task.WhenAll(publishTasks);

                await Task.Delay(DelayTime);
            }
        }

        static async Task PublishRoomEventsAsync(RoomState room)
        {
            try
            {
                if (room.Action == RoomAction.Leave)
                {
                    if (!room.TakenSeats.Any())
                    {
                        room.Action = RoomAction.Enter;
                    }
                    else
                    {
                        await LeaveSeatsAsync(room, random.Next(1, MaxLeaveSeatCount));
                    }
                }
                else
                {
                    if (!room.EmptySeats.Any())
                    {
                        room.Action = RoomAction.Leave;
                    }
                    else
                    {
                        await TakeSeatsAsync(room, random.Next(1, MaxTakeSeatCount));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish events for room '{room.RoomId}'. Details: {ex}");
            }
        }

        private static async Task TakeSeatsAsync(RoomState room, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!room.EmptySeats.Any())
                {
                    return;
                }

                await TakeSeatAsync(room);
            }
        }

        private static Task TakeSeatAsync(RoomState room)
        {
            // Pick an empty seat.
            var seatNumber = room.EmptySeats[random.Next(room.EmptySeats.Count)];

            // Mark the seat as taken.
            room.EmptySeats.Remove(seatNumber);
            room.TakenSeats.Add(seatNumber);

            return room.EventPublisher.PublishEventsAsync(room.RoomId, seatNumber, true);
        }

        private static async Task LeaveSeatsAsync(RoomState room, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!room.TakenSeats.Any())
                {
                    return;
                }

                await LeaveSeatAsync(room);
            }
        }

        private static Task LeaveSeatAsync(RoomState room)
        {
            // Pick an occupied seat.
            var seatNumber = room.TakenSeats[random.Next(room.TakenSeats.Count)];

            // Mark the seat as taken.
            room.TakenSeats.Remove(seatNumber);
            room.EmptySeats.Add(seatNumber);

            return room.EventPublisher.PublishEventsAsync(room.RoomId, seatNumber, false);
        }
    }
}
