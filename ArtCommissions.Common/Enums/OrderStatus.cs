namespace ArtCommissions.Common.Enums
{
    public enum OrderStatus: sbyte
    {
        REQUEST,
        WAITING_PAYMENT,
        IN_PROGRESS,
        FINISHED,
        CANCELLED = -1
    }
}
