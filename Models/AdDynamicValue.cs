using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class AdDynamicValue
{
    public int Id { get; set; }

    public int AdId { get; set; }

    public int FieldId { get; set; }

    public string? FieldValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Ad Ad { get; set; } = null!;

    public virtual DynamicField Field { get; set; } = null!;
}
