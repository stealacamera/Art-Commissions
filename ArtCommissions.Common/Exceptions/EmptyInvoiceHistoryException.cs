namespace ArtCommissions.Common.Exceptions;

public class EmptyInvoiceHistoryException : ArtCommissionsException
{
    public EmptyInvoiceHistoryException(string message) : base(message) { }
}
