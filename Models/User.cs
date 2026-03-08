using System;
using System.Collections.Generic;

namespace O_market.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string FullName { get; set; } = null!;

    public string? Otp { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public bool IsVerified { get; set; }

    public int? OtpAttempts { get; set; }

    public DateTime? OtpSentAt { get; set; }

    public virtual ICollection<Ad> Ads { get; set; } = new List<Ad>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<UserAdActivity> UserAdActivities { get; set; } = new List<UserAdActivity>();
}
