namespace RabbitProducer
{
    public class Event
    {
        public int reporterId { get; set; }
        public DateTime timestamp { get; set; }
        public int metricId { get; set; }
        public int metricValue { get; set; }
        public string message { get; set; }

        public Event(int reporterId, DateTime timestamp, int metricId, int metricValue, string message)
        {
            this.reporterId = reporterId;
            this.timestamp = timestamp;
            this.metricId = metricId;
            this.metricValue = metricValue;
            this.message = message;
        }
    }
}
