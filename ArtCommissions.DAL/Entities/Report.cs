using ArtCommissions.Common.Enums;

namespace ArtCommissions.DAL.Entities;

public abstract class Report : BaseEntity
{
    public ReportStatus Status { get; set; }
    public string Reason { get; set; }
}

public class UserReport : Report
{
    public int ReportedUserId { get; set; }
}

public class CommissionReport : Report
{
    public int ReportedCommissionId { get; set; }
}