using O_market.Models;
using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs
{
    public class AdResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceKm { get; set; }

        // Foreign Key related data
        public int UserId { get; set; }
        public string Username { get; set; } = null!;

        // NEW: Seller verification
        public bool IsSellerVerified { get; set; } = false;



        public string? SellerBadge { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        // Image URLs
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? PrimaryImageUrl { get; set; }

        // NEW: Time ago formatting
        public string? PostedTimeAgo { get; set; }

        // NEW: Is favorited by current user
        public bool IsFavorited { get; set; }

        // NEW: Short location (just city name)
        public string? ShortLocation { get; set; }

        // NEW: Featured badge
        public bool IsFeatured { get; set; }

        // NEW: Dynamic field highlights
        public Dictionary<string, string> Highlights { get; set; } = new();
    }
}