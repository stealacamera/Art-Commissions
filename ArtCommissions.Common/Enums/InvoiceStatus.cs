namespace ArtCommissions.Common.Enums;

public enum InvoiceStatus : sbyte
{
    WAITING_PAYMENT,
    PAID,
    CANCELLED = -1
}
