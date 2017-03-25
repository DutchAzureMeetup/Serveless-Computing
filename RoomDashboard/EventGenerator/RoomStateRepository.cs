using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;

namespace EventGenerator
{
    public class RoomStateRepository
    {
        private static readonly TimeSpan CacheRefreshTime = TimeSpan.FromSeconds(10);

        private readonly RoomRepository _roomRepository;
        private readonly Dictionary<Guid, RoomState> _cachedRooms;
        private DateTime _lastCacheUpdateTime;

        public RoomStateRepository(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            _cachedRooms = new Dictionary<Guid, RoomState>();
        }

        public async Task<IEnumerable<RoomState>> GetRoomsAsync()
        {
            if (DateTime.Now - _lastCacheUpdateTime > CacheRefreshTime)
            {
                await RefreshCacheAsync();
            }

            return _cachedRooms.Values;
        }

        private async Task RefreshCacheAsync()
        {
            var registeredRooms = (await _roomRepository.GetRoomsAsync()).ToList();

            // Remove all cached rooms that do not exist anymore.
            foreach (var cachedRoomId in _cachedRooms.Keys)
            {
                if (!registeredRooms.Any(r => r.RoomId == cachedRoomId))
                {
                    _cachedRooms.Remove(cachedRoomId);
                }
            }

            // Add each new room to the cache.
            foreach (var room in registeredRooms)
            {
                if (!_cachedRooms.ContainsKey(room.RoomId))
                {
                    try
                    {
                        _cachedRooms.Add(room.RoomId, new RoomState
                        {
                            RoomId = room.RoomId,
                            EventPublisher = new SeatEventPublisher(room.QueueConnectionString, room.QueueName)
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create RoomState for room '{room.RoomId}'. Details: {ex}");
                    }
                }
            }

            _lastCacheUpdateTime = DateTime.Now;
        }
    }
}