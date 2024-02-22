namespace ArtCommissions.Common.Exceptions;

public class ExistingUnpaidInvoiceException : ArtCommissionsException
{
    public ExistingUnpaidInvoiceException() : base("You cannot create a new invoice without paying or cancelling the current open invoice") { }
}