using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using EventGateway.Core;

namespace EventGateway.RestClient
{
    /// <summary>
    /// publishes events through API proxy
    /// </summary>
    public class RestApiMessageProcessor : IMessageProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger<RestApiMessageProcessor>();

        private readonly HttpClient _client = new HttpClient();
        private const string MessagesResource = "messages";
        private const int TimeoutSeconds = 20;

        public RestApiMessageProcessor(string baseUrl)
        {
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            _client.BaseAddress = new Uri(baseUrl);
            _client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        }

        public async Task<bool> Process(Message message)
        {
            try
            {

                Logger.Debug(m => m($"Posting message: {message}"));

                var content = new StringContent(message.ToJsonString(), Encoding.UTF8, "application/json");
                var resourceUri = new Uri(_client.BaseAddress, MessagesResource);
                var response = await _client.PostAsync(resourceUri, content);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    Logger.Warn(m => m($"Invalid response code for {message}: {response.StatusCode}"));
                    return false;
                }

                Logger.Debug(m => m($"Message {message} sent successfully"));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(m => m($"Error while processing {message}"), ex);
                return false;
            }
        }
    }
}
