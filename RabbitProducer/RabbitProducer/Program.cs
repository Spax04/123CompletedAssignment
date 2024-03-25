using RabbitMQ.Client;
using RabbitProducer;
using System.Text;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

internal class Program
{
    private static void Main(string[] args)
    {



        try
        {
            // Create an instance of the logger
            var logger = Logger.Instance;

            // Deserialize YAML file
            YamlMappingNode yamlObject = ConfigurationLoader.LoadConfiguration();

            // Extract RabbitMQ configuration
            var rabbitMQConfig = yamlObject["RabbitMQ"] as YamlMappingNode;
            var eventConfig = yamlObject["Event"] as YamlMappingNode;

            if (rabbitMQConfig != null && eventConfig != null)
            {
                // Create RabbitMQ connection factory with loaded configuration
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMQConfig["host"].ToString(),
                    Port = int.Parse(rabbitMQConfig["port"].ToString()), // Ensure correct type conversion
                    ClientProvidedName = rabbitMQConfig["producer_name"].ToString()
                };

                // Create connection and channel
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    string routingKey = rabbitMQConfig["routing_key"].ToString();
                    string queue = rabbitMQConfig["queue"].ToString();

                    // Declare exchange
                    string exchangeName = rabbitMQConfig["exchange"].ToString();
                    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                    channel.QueueDeclare(queue, false, false, false, null);
                    channel.QueueBind(queue, exchangeName, routingKey, null);
                    // Initialize reporter_id
                    int globalReporterId = int.Parse(eventConfig["initial_reporter_id"].ToString()); // Ensure correct type conversion

                    while (true)
                    {
                        // Create an instance of the Event class
                        var eventObject = new Event();
                        eventObject.ReporterId = globalReporterId;
                        var eventJson = JsonSerializer.Serialize(eventObject); // Serialize event object to JSON

                        Console.WriteLine($"Producing event: {eventJson}");

                        var body = Encoding.UTF8.GetBytes(eventJson);

                        // Publish the event to the exchange with the routing key

                        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);

                        // Increment reporter_id for the next event
                        globalReporterId += int.Parse(eventConfig["reporter_id_increment"].ToString()); // Ensure correct type conversion

                        // wait before producing the next event
                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(eventConfig["sleep_time_seconds"].ToString()))); // Ensure correct type conversion
                    }
                }
            }
            else
            {
                Logger.Instance.LogError($"Error: RabbitMQ or Event configuration not found in YAML file.");
                Console.WriteLine("Error: RabbitMQ or Event configuration not found in YAML file.");
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
