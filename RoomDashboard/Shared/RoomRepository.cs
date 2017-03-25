using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;

namespace Shared
{
    public class RoomRepository
    {
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _table;
     
        public RoomRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            _tableClient = storageAccount.CreateCloudTableClient();
            _table = _tableClient.GetTableReference("seats");
            _table.CreateIfNotExists();
        }

        public Task AddRoomAsync(string roomName, string queueConnectionString, string queueName)
        {
            var entity = new DynamicTableEntity(
                "Rooms",
                Guid.NewGuid().ToString(),
                "*",
                new Dictionary<string, EntityProperty>
                {
                    { "RName", EntityProperty.GeneratePropertyForString(roomName) },
                    { "QConn", EntityProperty.GeneratePropertyForString(queueConnectionString) },
                    { "QName", EntityProperty.GeneratePropertyForString(queueName) }
                });

            return _table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async Task<IEnumerable<Room>> GetRoomsAsync()
        {
            var query = _table.CreateQuery<DynamicTableEntity>()
                .Where(e => e.PartitionKey == "Rooms")
                .AsTableQuery();

            var result = await query.ExecuteSegmentedAsync(null);
            if (result != null)
            {
                return result.Select(e => new Room
                {
                    RoomId = Guid.Parse(e.RowKey),
                    RoomName = e.Properties["RName"].StringValue,
                    QueueConnectionString = e.Properties["QConn"].StringValue,
                    QueueName = e.Properties["QName"].StringValue
                });
            }

            return Enumerable.Empty<Room>();
        }
    }
}