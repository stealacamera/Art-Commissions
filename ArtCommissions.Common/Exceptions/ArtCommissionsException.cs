namespace ArtCommissions.Common.Exceptions
{
    public class ArtCommissionsException : Exception
    {
        public ArtCommissionsException() : base() { }
        public ArtCommissionsException(string message) : base(message) { }
    }
}
