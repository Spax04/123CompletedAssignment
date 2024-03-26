using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RabbitProducer
{
    public class ConfigurationLoader
    {
        private static readonly Lazy<ConfigurationLoader> instance = new Lazy<ConfigurationLoader>(() => new ConfigurationLoader());
        public static ConfigurationLoader Instance => instance.Value;

        private readonly YamlMappingNode configuration;

        private ConfigurationLoader()
        {
            configuration = LoadConfiguration();
        }

        // Load configuration from file
        private YamlMappingNode LoadConfiguration()
        {
            try
            {
                var deserializer = new DeserializerBuilder().Build();
                using (var streamReader = new StreamReader("./configuration.yaml"))
                {
                    return deserializer.Deserialize<YamlMappingNode>(streamReader);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Error loading configuration: {ex.Message}");
                Console.Error.WriteLine(ex.ToString());
                throw new Exception($"Error loading configuration: {ex.Message}");
            }
        }

        // Accessor method to get the loaded configuration
        public YamlMappingNode GetConfiguration()
        {
            return configuration;
        }
    }
}
