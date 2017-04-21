using Common.Logging;
using Contour;
using EventGateway.Core;

namespace EventGateway.CP
{
    public class MessageListener : IMessageListener
    {
        private static readonly ILog Logger = LogManager.GetLogger<MessageListener>();

        private readonly IBus _bus;

        public MessageListener(IBus bus)
        {
            _bus = bus;
        }

        public void Start()
        {
            Logger.Info(m => m("Starting bus listener..."));
            _bus.Start();
            Logger.Info(m => m("Bus listener is started"));
        }

        public void Stop()
        {
            Logger.Info(m => m("Stoping bus listener..."));
            _bus.Stop();
            Logger.Info(m => m("Bus listener is stoped"));
        }
    }
}
