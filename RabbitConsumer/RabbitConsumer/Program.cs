using Couchbase;
using Couchbase.Management.Buckets;
using Couchbase.Management.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RabbitConsumer
{
    internal class Program
    {

        private static async Task Main(string[] args)
        {
            ConnectionFactory factory = null;
            IConnection connection = null;
            IModel channel = null;
            try
            {
                // Deserialize YAML file
                var deserializer = new DeserializerBuilder().Build();
                YamlMappingNode yamlObject = ConfigurationLoader.LoadConfiguration();

                // Extract RabbitMQ configuration
                var rabbitMQConfig = yamlObject["RabbitMQ"] as YamlMappingNode;
                var eventConfig = yamlObject["Event"] as YamlMappingNode;
                var couchbaseConfig = yamlObject["CoucheBase"] as YamlMappingNode;

                if (rabbitMQConfig != null && eventConfig != null && couchbaseConfig != null)
                {
                    // Connect to Couchbase
                    var couchbaseUri = couchbaseConfig["couchbase_uri"].ToString();
                    var options = new ClusterOptions
                    {
                        UserName = couchbaseConfig["username"].ToString(),
                        Password = couchbaseConfig["user_password"].ToString(),
                    };
                    options.ApplyProfile("wan-development");

                    var cluster = await Cluster.ConnectAsync(couchbaseUri, options);
                    await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
                    IBucket bucket = null;
                    try
                    {
                        // Attempt to get the bucket
                        bucket = await cluster.BucketAsync(couchbaseConfig["bucket_name"].ToString());
                    }
                    catch
                    {
                        // Bucket not found, create it
                        var bucketManager = cluster.Buckets;
                        var bucketSettings = new BucketSettings
                        {
                            Name = couchbaseConfig["bucket_name"].ToString(),
                            BucketType = BucketType.Couchbase,
                            RamQuotaMB = 100, // Set the appropriate RAM quota
                            FlushEnabled = false,
                            ReplicaIndexes = false,
                            ConflictResolutionType = ConflictResolutionType.Timestamp
                        };
                        await bucketManager.CreateBucketAsync(bucketSettings);
                        await Task.Delay(3000);
                        // Retry to get the bucket
                        bucket = await cluster.BucketAsync(couchbaseConfig["bucket_name"].ToString());
                    }

                    // Creating Primary Index
                    await cluster.QueryIndexes.CreatePrimaryIndexAsync(
                    couchbaseConfig["bucket_name"].ToString(),
                    options => options.IgnoreIfExists(true)
                        );

                    await cluster.QueryIndexes.CreateIndexAsync(
                couchbaseConfig["bucket_name"].ToString(),
                "epochDateIndex",
              new[] { "epochDate" },
             options => options.IgnoreIfExists(true)
                        );


                    // Create RabbitMQ connection factory with loaded configuration
                    factory = new ConnectionFactory
                    {
                        HostName = rabbitMQConfig["host"].ToString(),
                        Port = int.Parse(rabbitMQConfig["port"].ToString()), // Ensure correct type conversion
                        ClientProvidedName = rabbitMQConfig["consumer_name"].ToString()
                    };

                    // Create connection and channel
                    connection = factory.CreateConnection();
                    channel = connection.CreateModel();

                    string queue = rabbitMQConfig["queue"].ToString();
                    channel.QueueDeclare(queue, true, false, false, null);
                    channel.BasicQos(0, 1, false);

                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("Consumer created");
                    consumer.Received += async (model, ea) =>
                     {
                         var body = ea.Body.ToArray();
                         var message = Encoding.UTF8.GetString(body);
                         Console.WriteLine("Received message: {0}", message);

                         // Deserialize JSON message to dynamic object
                         var document = JsonConvert.DeserializeObject<dynamic>(message);
                         DateTime timestamp = DateTime.Parse(document.timestamp.ToString());


                         // Create a JObject directly
                         JObject serializedDate = new JObject
                         {
                                 { "stringDate", timestamp.ToString("yyyy-MM-ddTHH:mm:ss") },
                                 { "epochDate", new DateTimeOffset(timestamp).ToUnixTimeSeconds() }
                         };

                         // Assign the serialized date to Timestamp
                         document.timestamp = serializedDate;

                         // Store the message in Couchbase
                         var documentId = Guid.NewGuid().ToString(); // Generate a unique document ID
                         try
                         {
                             await bucket.DefaultCollection().UpsertAsync(documentId, document);
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(ex.ToString());
                         }

                         Console.WriteLine("Inserted document with ID: {0}", documentId);
                     };

                    // Start consuming
                    channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);

                    Console.WriteLine("Consumer started.");


                    // Handles Ctrl+C signal to gracefully shutdown the application.
                    var waitHandle = new ManualResetEventSlim(false);
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true; // Prevent default Ctrl+C behavior
                        waitHandle.Set(); // Signal exit
                    };
                    waitHandle.Wait();
                }

                else
                {
                    Logger.Instance.LogError($"Error: RabbitMQ or Event configuration not found in YAML file.");
                    Console.WriteLine("Error: RabbitMQ, Event, or Couchbase configuration not found in YAML file.");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Dispose of connection and channel
                if (channel != null && channel.IsOpen)
                {
                    channel.Close();
                    channel.Dispose();
                }
                if (connection != null && connection.IsOpen)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}
