using System;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using EventGateway.Core;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace EventGateway.Azure
{
    /// <summary>
    /// publishes events directly to Azure Event Hub
    /// </summary>
    public class EventHubMessageProcessor : IMessageProcessor, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger<EventHubMessageProcessor>();

        private readonly EventHubClient _eventHubClient;

        public EventHubMessageProcessor(string connectionString, string entityPath)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString)
            {
                EntityPath = entityPath
            };

            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        public async Task Process(Message message)
        {
            Logger.Debug(m => m($"Processing {message}"));

            try
            {
                // convert Message to EventData
                var eventData = ToEventData(message);

                await _eventHubClient.SendAsync(eventData);

                Logger.Debug(m => m($"Uploaded successfully {message}"));
            }
            catch (Exception ex)
            {
                Logger.Error(m => m("Error while sending to hub"), ex);
            }
        }

        public void Dispose()
        {
            _eventHubClient.Close();
        }

        private EventData ToEventData(Message message)
        {
            var bodyBytes = ToByteArray(message.Payload);
            var eventData = new EventData(bodyBytes)
            {
                Properties =
                    {
                        ["Label"] = message.Label
                    }
            };

            if (message.Headers != null)
            {
                foreach (var key in message.Headers.Keys)
                {
                    var val = message.Headers[key];
                    Logger.Trace(m => m($"Message header: {key} -> {val}"));
                    eventData.Properties[key] = message.Headers[key];
                }
            }

            return eventData;
        }

        private byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;

            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Logger.Trace(m => m("Message body:\n{0}", json));
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
