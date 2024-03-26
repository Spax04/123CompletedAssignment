using Couchbase;
using Couchbase.Linq;
using StackExchange.Redis;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RedisETL
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {

                // Deserialize YAML file
                var deserializer = new DeserializerBuilder().Build();
                YamlMappingNode yamlObject = ConfigurationLoader.LoadConfiguration();

                // Extract RabbitMQ configuration
                var eventConfig = yamlObject["Event"] as YamlMappingNode;
                var couchbaseConfig = yamlObject["CoucheBase"] as YamlMappingNode;
                var redisHost = yamlObject["Redis"]["host"].ToString();
                var redisPort = int.Parse(yamlObject["Redis"]["port"].ToString());

                if (eventConfig != null && couchbaseConfig != null)
                {
                    // Connect to Couchbase
                    var couchbaseUri = couchbaseConfig["couchbase_uri"].ToString();
                    var options = new ClusterOptions
                    {
                        UserName = couchbaseConfig["username"].ToString(),
                        Password = couchbaseConfig["user_password"].ToString(),

                    }.AddLinq().ApplyProfile("wan-development");


                    var cluster = await Cluster.ConnectAsync(couchbaseUri, options);

                    var bucket = await cluster.BucketAsync(couchbaseConfig["bucket_name"].ToString());
                    var context = new BucketContext(bucket);
                    var context1 = new BucketContext(await cluster.BucketAsync("EventBucket"));



                    Console.WriteLine("Radis is running");

                    while (true)
                    {
                        Console.WriteLine("starting to get data");
                        var redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
                        var redisDb = redis.GetDatabase();
                        await RadisHelper.ExtractTransformLoad(redisDb, context, yamlObject["Event"]["timestamp_format"].ToString());
                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(yamlObject["Redis"]["sleep_time_seconds"].ToString())));
                    }

                }
                else
                {
                    Logger.Instance.LogError($"Error: Couchbase,Redis or Event configuration not found in YAML file.");
                    Console.WriteLine("Error:Couchbase,Redis or Event configuration not found in YAML file.");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


    }
}
