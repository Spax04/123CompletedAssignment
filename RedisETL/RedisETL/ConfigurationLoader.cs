﻿using System.Reflection;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RedisETL
{
    public class ConfigurationLoader
    {

        public static YamlMappingNode LoadConfiguration()
        {

            try
            {
                string projectDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                string yamlFilePath = "./configuration.yaml";
                var deserializer = new DeserializerBuilder().Build();
                YamlMappingNode yamlObject = deserializer.Deserialize<YamlMappingNode>(new StreamReader(yamlFilePath));
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
