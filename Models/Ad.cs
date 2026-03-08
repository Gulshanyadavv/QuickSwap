using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class Ad
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Location { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public virtual ICollection<AdDynamicValue> AdDynamicValues { get; set; } = new List<AdDynamicValue>();

    public virtual ICollection<AdImage> AdImages { get; set; } = new List<AdImage>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserAdActivity> UserAdActivities { get; set; } = new List<UserAdActivity>();
}
