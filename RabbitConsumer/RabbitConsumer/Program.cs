using Couchbase;
using Newtonsoft.Json;
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
                    var bucket = await cluster.BucketAsync(couchbaseConfig["bucket_name"].ToString());

                    // Create RabbitMQ connection factory with loaded configuration
                    var factory = new ConnectionFactory
                    {
                        HostName = rabbitMQConfig["host"].ToString(),
                        Port = int.Parse(rabbitMQConfig["port"].ToString()), // Ensure correct type conversion
                        ClientProvidedName = rabbitMQConfig["consumer_name"].ToString()
                    };

                    // Create connection and channel
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        string queue = rabbitMQConfig["queue"].ToString();
                        channel.QueueDeclare(queue, false, false, false, null);
                        channel.BasicQos(0, 1, false);

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine("Received message: {0}", message);

                            // Deserialize JSON message to dynamic object
                            var document = JsonConvert.DeserializeObject<dynamic>(message);



                            // Store the message in Couchbase
                            var documentId = Guid.NewGuid().ToString(); // Generate a unique document ID
                            await bucket.DefaultCollection().UpsertAsync(documentId, document);

                            Console.WriteLine("Inserted document with ID: {0}", documentId);
                        };

                        // Start consuming
                        channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);

                        Console.WriteLine("Consumer started. Press [Enter] to exit.");
                        Console.ReadLine();
                    }
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
        }
    }
}
