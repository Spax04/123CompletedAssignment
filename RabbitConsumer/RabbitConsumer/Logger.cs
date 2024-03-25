using System.Diagnostics;

namespace RabbitConsumer
{
    public class Logger
    {
        private static readonly Lazy<Logger> lazyLogger = new Lazy<Logger>(() => new Logger()); //!
        private readonly TextWriterTraceListener listener;

        public static Logger Instance => lazyLogger.Value;

        private Logger()
        {
            // Get the name of the current directory (project folder name)
            string folderName = Path.GetFileName(Directory.GetCurrentDirectory());

            // Create a TextWriterTraceListener to write logs to a file
            string logFilePath = $"{folderName}.log";
            listener = new TextWriterTraceListener(logFilePath);

            // Add the TextWriterTraceListener to the Trace listeners collection
            Trace.Listeners.Add(listener);

            // Set the TraceSwitch level
            TraceSwitch traceSwitch = new TraceSwitch("TraceLevel", "Trace Level Switch");
            traceSwitch.Level = TraceLevel.Info; // Change as needed
        }

        public void LogInfo(string message)
        {
            Trace.TraceInformation(message);
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }



        public void Close()
        {
            // Flush and close the TextWriterTraceListener
            listener.Flush();
            listener.Close();
        }
    }
}
