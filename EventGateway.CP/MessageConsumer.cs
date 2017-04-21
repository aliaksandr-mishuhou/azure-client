using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Contour.Receiving;
using Contour.Receiving.Consumers;
using EventGateway.Core;
using Newtonsoft.Json.Linq;

namespace EventGateway.CP
{
    public class MessageConsumer : IConsumerOf<JObject>
    {
        private static readonly ILog Logger = LogManager.GetLogger<MessageConsumer>();

        private const string SourceKey = "Source";

        private readonly IEnumerable<IMessageProcessor> _messageProcessors;

        public MessageConsumer(IEnumerable<IMessageProcessor> messageProcessors)
        {
            _messageProcessors = messageProcessors;
        }

        public async void Handle(IConsumingContext<JObject> context)
        {
            Logger.Debug(m => m($"Processing message with label {context.Message.Label}"));

            try
            {
                var message = new Message
                {
                    Label = context.Message.Label.Name,
                    Headers = context.Message.Headers.ToDictionary(kv => kv.Key, kv => GetHeaderValue(kv.Value)),
                    Payload = context.Message.Payload
                };

                message.Headers[SourceKey] = context.Bus.Endpoint.Address;

                foreach (var processor in _messageProcessors)
                {
                    try
                    {
                        await processor.Process(message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(m => m($"Error while processing message {message.Label} with {processor.GetType().Name}"), ex);
                    }
                }

                context.Accept();
            }
            catch (Exception ex)
            {
                Logger.Error(m => m($"Error while processing message {context.Message}"), ex);
                context.Reject(false);
            }
        }

        private static string GetHeaderValue(object val)
        {
            var bytes = val as byte[];
            return bytes != null ? Encoding.UTF8.GetString(bytes) : null;
        }
    }
}
