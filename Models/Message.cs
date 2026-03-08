using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public int? AdId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public string? AttachmentUrl { get; set; }

    public string? AttachmentType { get; set; }

    public virtual Ad? Ad { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
