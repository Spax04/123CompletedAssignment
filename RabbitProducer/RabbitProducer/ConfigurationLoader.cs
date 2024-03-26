﻿using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RabbitProducer
{
    public class ConfigurationLoader
    {
        public static YamlMappingNode LoadConfiguration()
        {

            try
            {
                var deserializer = new DeserializerBuilder().Build();
                YamlMappingNode yamlObject = deserializer.Deserialize<YamlMappingNode>(new StreamReader("./configuration.yaml"));
                return yamlObject;

            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Error loading configuration: {ex.Message}");
                Console.Error.WriteLine(ex.ToString());
                throw new Exception($"Error loading configuration: {ex.Message}");
            }
        }
    }
}
