namespace ArtCommissions.Common.Exceptions;

public class ExistingReviewForCommissionException : ArtCommissionsException
{
    public ExistingReviewForCommissionException() : base("You cannot add more than one review to a commission") { }
}
