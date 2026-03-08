public class DynamicFieldDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Label { get; set; } = null!;
    public string FieldType { get; set; } = null!;  // Text, Number, Dropdown, Boolean
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
}
