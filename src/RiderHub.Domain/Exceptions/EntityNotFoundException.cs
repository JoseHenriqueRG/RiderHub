namespace RiderHub.Domain.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public int EntityId { get; }

        public EntityNotFoundException(string entityName, int entityId)
            : base($"{entityName} with ID {entityId} was not found.")
        {
            EntityId = entityId;
        }
    }
}
