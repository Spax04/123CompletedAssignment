using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RabbitProducer
{
    public class Event
    {
        public int ReporterId { get; set; }
        public DateTime Timestamp { get; set; }
        public int MetricId { get; set; }
        public int MetricValue { get; set; }
        public string Message { get; set; }

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
                ReporterId = Convert.ToInt32(eventConfig["initial_reporter_id"].ToString());
                MetricId = GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_id"].ToString()));
                MetricValue = GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_value"].ToString()));
                Message = eventConfig["message"].ToString();

                // Set the timestamp
                Timestamp = DateTime.Now;

                // Format the timestamp using the specified format from the configuration
                string timestampFormat = eventConfig["timestamp_format"].ToString();
                Timestamp = DateTime.ParseExact(Timestamp.ToString(timestampFormat), timestampFormat, null);

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
