namespace ArtCommissions.Common.DTOs;

public class CommissionSampleImage
{
    public int Id { get; set; }
    public string ImageName { get; set; } = null!;
}

public class CommissionSampleImageUpdateRequestModel : CommissionSampleImage
{
    public bool ShouldRemove { get; set; } = false;
}