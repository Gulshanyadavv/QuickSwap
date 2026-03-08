using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class DynamicField
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Label { get; set; } = null!;

    public string FieldType { get; set; } = null!;

    public string? Options { get; set; }

    public bool? IsRequired { get; set; }

    public virtual ICollection<AdDynamicValue> AdDynamicValues { get; set; } = new List<AdDynamicValue>();

    public virtual Category Category { get; set; } = null!;
}
