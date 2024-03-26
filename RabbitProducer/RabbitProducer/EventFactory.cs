namespace RabbitProducer
{
    public class EventFactory
    {
        private readonly ConfigurationLoader configurationLoader;

        public EventFactory()
        {
            this.configurationLoader = ConfigurationLoader.Instance;
        }

        public Event CreateEvent()
        {
            try
            {
                // Access configuration properties
                var yamlObject = configurationLoader.GetConfiguration();
                var eventConfig = yamlObject["Event"];

                // Create a new Event object with configured properties
                var eventObject = new Event(
                Convert.ToInt32(eventConfig["initial_reporter_id"].ToString()),
                GetLocalTimeWithMilliseconds(eventConfig["timestamp_format"].ToString()),
                GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_id"].ToString())),
                GetRandomInt(Convert.ToInt32(eventConfig["min_metric_id"].ToString()), Convert.ToInt32(eventConfig["max_metric_value"].ToString())),
                eventConfig["message"].ToString()
                );

                // Log successful event creation
                Logger.Instance.LogInfo("Event created successfully.");

                return eventObject;
            }
            catch (Exception ex)
            {
                // Log error creating event
                Console.Error.WriteLine(ex.ToString());
                Logger.Instance.LogError($"Error creating event: {ex.Message}");
                return null;
            }
        }

        private int GetRandomInt(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue + 1);
        }

        private DateTime GetLocalTimeWithMilliseconds(string format)
        {
            // Get the current UTC time with milliseconds
            DateTime utcTimeWithMilliseconds = DateTime.UtcNow;

            // Convert UTC time to local time zone and preserve the format
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTimeWithMilliseconds, TimeZoneInfo.Local);

            // Parse the local time to string using the specified format and then parse it back to DateTime
            string localTimeString = localTime.ToString(format);
            return DateTime.ParseExact(localTimeString, format, null);
        }
    }
}
