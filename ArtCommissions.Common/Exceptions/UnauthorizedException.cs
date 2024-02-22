namespace ArtCommissions.Common.Exceptions
{
    public class UnauthorizedException : ArtCommissionsException
    {
        public UnauthorizedException() : base("You do not have permission to perform this action.") { }
    }
}
