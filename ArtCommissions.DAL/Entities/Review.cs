namespace ArtCommissions.DAL.Entities
{
    public class Review
    {
        public int ReviewerId { get; set; }
        public int CommissionId { get; set; }
        internal Commission Commission { get; set; }

        private int _rating;
        public int Rating 
        {
            get => _rating;
            set 
            {
                if (value < 1 || value > 5) throw new Exception("Rating has to be between 1-5");
                _rating = value;
            }
        }
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}
