namespace O_market.DTOs
{
    public class AdUpdateDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public string? Location { get; set; }

        public int? CategoryId { get; set; }

        // Allow user to change status (e.g., from 'Active' to 'Expired')
        public string? Status { get; set; }
    }
}
