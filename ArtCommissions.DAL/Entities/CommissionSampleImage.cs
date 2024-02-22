namespace ArtCommissions.DAL.Entities
{
    public class CommissionSampleImage : BaseEntity
    {
        public int CommissionId { get; set; }
        public string ImagePath { get; set; } = null!;
    }
}
