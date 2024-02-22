namespace ArtCommissions.Common.DTOs.ViewModels
{
    public class ImageCarouselVM
    {
        public List<CommissionSampleImage> SampleImages { get; set; } = null!;
        public int CommissionId { get; set; }
    }
}
