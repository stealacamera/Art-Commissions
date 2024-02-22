using ArtCommissions.Common.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public abstract class Report
{
    public int Id { get; set; }

    public ReportStatus Status { get; set; }

    public string Reason { get; set; } = null!;
}

public class UserReport : Report
{
    public AppUser ReportedUser { get; set; } = null!;
}

public class CommissionReport : Report
{
    public Commission ReportedCommission { get; set; } = null!;
}

public abstract class ReportAddRequestModel
{
    [Required(ErrorMessage = "A reason needs to be given for this report")]
    [StringLength(1000, ErrorMessage = "This message cannot exceed 1000 characterss")]
    public string Reason { get; set; } = null!;
}

public class UserReportAddRequestModel : ReportAddRequestModel
{
    [ValidateNever]
    public int ReportedUserId { get; set; }
}

public class CommissionReportAddRequestModel : ReportAddRequestModel
{
    [ValidateNever]
    public int ReportedCommissionId { get; set; }
}