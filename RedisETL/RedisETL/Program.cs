using Couchbase;
using Couchbase.KeyValue;
using Newtonsoft.Json;
using StackExchange.Redis;
using YamlDotNet.RepresentationModel;

namespace RedisETL
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Load configuration from YAML
                var yamlObject = ConfigurationLoader.LoadConfiguration();

                // Extract Couchbase configuration
                var couchbaseConfig = yamlObject["CoucheBase"] as YamlMappingNode;
                var couchbaseUri = couchbaseConfig["couchbase_uri"].ToString();
                var bucketName = couchbaseConfig["bucket_name"].ToString();
                var username = couchbaseConfig["username"].ToString();
                var password = couchbaseConfig["user_password"].ToString();

                // Connect to Couchbase
                var options = new ClusterOptions
                {
                    UserName = username,
                    Password = password
                };
                options.ApplyProfile("wan-development");
                var cluster = await Cluster.ConnectAsync(couchbaseUri, options);
                var bucket = await cluster.BucketAsync(bucketName);

                // Extract Redis configuration
                var redisConfig = yamlObject["Redis"] as YamlMappingNode;
                var redisHost = redisConfig["host"].ToString();
                var redisPort = int.Parse(redisConfig["port"].ToString());

                // Connect to Redis
                var redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
                var redisDb = redis.GetDatabase();

                while (true)
                {
                    ExtractTransformLoad(bucket, redisDb);
                    // Sleep for specified time
                    var sleepTimeSeconds = int.Parse(redisConfig["sleep_time_seconds"].ToString());
                    await Task.Delay(sleepTimeSeconds * 1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ExtractTransformLoad(IBucket bucket, IDatabase redisDb)
        {
            // Extract data from Couchbase
            var result = bucket.DefaultCollection().Get("example_key");
            if (result != null)
            {
                var data = result.ContentAs<dynamic>();

                // Transform data if needed

                // Load data into Redis
                var redisKey = data["reporterId"].ToString();
                var redisValue = JsonConvert.SerializeObject(data);
                redisDb.StringSet(redisKey, redisValue);

                Console.WriteLine($"Data stored in Redis with key: {redisKey}");
            }
            else
            {
                Console.WriteLine("No data found in Couchbase.");
            }
        }
    }
}
