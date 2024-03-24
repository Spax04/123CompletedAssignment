namespace RabbitProducer
{
    public class Event
    {
        private int _reporterId;
        private DateTime _timestamp;
        private int _metricId;
        private int _metricValue;
        private string _message;


        public Event()
        {
            _timestamp = DateTime.Now;
        }
    }
}
