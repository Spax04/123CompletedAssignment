namespace RedisETL.dto
{
    public class CouchbaseDocument
    {
        public int reporterId { get; set; }
        public Timestamp timestamp { get; set; } // Change the type to Timestamp
        public int metricId { get; set; }
        public int metricValue { get; set; }
        public string message { get; set; }
    }
}
