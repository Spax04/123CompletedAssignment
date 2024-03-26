using System.Runtime.Serialization;

namespace RabbitConsumer.dto
{
    [DataContract]
    public class Date
    {
        [DataMember(Name = "stringDate")]
        public string StringDate { get; set; }
        [DataMember(Name = "epochDate")]
        public long EpochDate { get; set; }
    }
}
