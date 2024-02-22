namespace ArtCommissions.Common.Exceptions
{
    public class EntityNotFoundException : ArtCommissionsException
    {
        private static string _messageBase = "could not be found";

        public EntityNotFoundException() : base($"Entity {_messageBase}") { }
        public EntityNotFoundException(string entityName) : base($"{entityName} {_messageBase}") { }
    }
}
