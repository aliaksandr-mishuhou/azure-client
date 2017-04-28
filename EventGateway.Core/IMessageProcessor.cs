using System.Threading.Tasks;

namespace EventGateway.Core
{
    /// <summary>
    /// enchances and publishes messages to external 3d party systems
    /// </summary>
    public interface IMessageProcessor
    {
        Task<bool> Process(Message message);
    }
}
