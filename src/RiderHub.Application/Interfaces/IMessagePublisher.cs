namespace RiderHub.Application.Interfaces
{
    public interface IMessagePublisher
    {
        void Publish(string message);
    }
}
