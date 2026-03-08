using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class UserAdActivity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AdId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Ad Ad { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
