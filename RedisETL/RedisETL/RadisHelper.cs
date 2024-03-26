using Couchbase.Linq;
using Newtonsoft.Json;
using RedisETL.dto;
using StackExchange.Redis;

namespace RedisETL
{


    public static class RadisHelper
    {
        public static async Task ExtractTransformLoad(IDatabase redisDb, IBucketContext context, string timestampFormat)
        {
            try
            {
                var latestTimestamp = await GetLatestTimestamp(redisDb);

                // Query the data using LINQ to Couchbase
                var query = context.Query<CouchbaseDocument>().Where(q => q.timestamp.epochDate > latestTimestamp).OrderBy(q => q.timestamp.epochDate); ;

                // Iterate through the results and prepare the Redis key-value pairs
                await foreach (var row in query.ToAsyncEnumerable())
                {

                    // Generate Redis key using reporterId and timestamp
                    var redisKey = $"{row.reporterId}:{row.timestamp.stringDate}";

                    // Convert data to JSON string
                    var redisValue = JsonConvert.SerializeObject(row);

                    // Set the key-value pair in Redis
                    await redisDb.StringSetAsync(redisKey, redisValue);

                    // Update latest timestamp in Redis

                    await SetLatestTimestamp(redisDb, row.timestamp.epochDate);
                    latestTimestamp = row.timestamp.epochDate;


                    Console.WriteLine($"Object with key {redisKey} inserted into Redis database");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExtractTransformLoad: {ex.Message}");
                // Handle the exception accordingly
            }
        }

        public static async Task<long> GetLatestTimestamp(IDatabase redisDb)
        {
            var latestTimestamp = await redisDb.StringGetAsync("latest_timestamp");
            return latestTimestamp.HasValue ? (long)latestTimestamp : 0;
        }

        public static async Task SetLatestTimestamp(IDatabase redisDb, long timestamp)
        {
            await redisDb.StringSetAsync("latest_timestamp", timestamp);
        }
    }
}
