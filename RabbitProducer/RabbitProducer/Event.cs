using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RabbitProducer
{
    public class Event
    {
        public int reporterId { get; set; }
        public DateTime timestamp { get; set; }
        public int metricId { get; set; }
        public int metricValue { get; set; }
        public string message { get; set; }

        public Event()
        {
            CreateEvent();
        }

        private void CreateEvent()
        {

            try
            {
                // Deserialize YAML file
                var deserializer = new DeserializerBuilder().Build();
                YamlMappingNode yamlObject = ConfigurationLoader.LoadConfiguration();

                // Access configuration properties
                var eventConfig = yamlObject["Event"];
                reporterId = Convert.ToInt32(eventConfig["initial_reporter_id"].ToString());
                metricId = GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_id"].ToString()));
                metricValue = GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_value"].ToString()));
                message = eventConfig["message"].ToString();

                // Set the timestamp
                timestamp = DateTime.Now;

                // Format the timestamp using the specified format from the configuration
                string timestampFormat = eventConfig["timestamp_format"].ToString();
                timestamp = DateTime.ParseExact(timestamp.ToString(timestampFormat), timestampFormat, null);

                // Log successful configuration loading
                Logger.Instance.LogInfo("Configuration loaded successfully.");
            }
            catch (Exception ex)
            {
                // Log error loading configuration
                Console.Error.WriteLine(ex.ToString());
                Logger.Instance.LogError($"Error loading configuration: {ex.Message}");
            }
        }

        private int GetRandomInt(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue + 1);
        }
    }
}
