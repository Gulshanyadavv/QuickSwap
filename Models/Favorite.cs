using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AdId { get; set; }

    public virtual Ad Ad { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
