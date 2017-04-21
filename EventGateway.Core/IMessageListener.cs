namespace EventGateway.Core
{
    public interface IMessageListener
    {
        void Start();

        void Stop();
    }
}
