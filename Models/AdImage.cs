using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class AdImage
{
    public int Id { get; set; }

    public int AdId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool? IsPrimary { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual Ad Ad { get; set; } = null!;
}
