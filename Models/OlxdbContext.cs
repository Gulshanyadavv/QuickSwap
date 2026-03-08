using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace O_market.Models;

public partial class OlxdbContext : DbContext
{
    public OlxdbContext()
    {
    }

    public OlxdbContext(DbContextOptions<OlxdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ad> Ads { get; set; }

    public virtual DbSet<AdDynamicValue> AdDynamicValues { get; set; }

    public virtual DbSet<AdImage> AdImages { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<DynamicField> DynamicFields { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAdActivity> UserAdActivities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=LAPTOP-LLKF91PS\\SQLEXPRESS;Database=OLXDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ads__3214EC07855BC821");

            entity.HasIndex(e => e.CategoryId, "IX_Ads_CategoryId");

            entity.HasIndex(e => e.CreatedAt, "IX_Ads_CreatedAt").IsDescending();

            entity.HasIndex(e => e.Location, "IX_Ads_Location");

            entity.HasIndex(e => e.Price, "IX_Ads_Price");

            entity.HasIndex(e => e.Status, "IX_Ads_Status");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_Ads_Status_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => e.UserId, "IX_Ads_UserId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.Ads)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ads__CategoryId__440B1D61");

            entity.HasOne(d => d.User).WithMany(p => p.Ads)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Ads__UserId__4316F928");
        });

        modelBuilder.Entity<AdDynamicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AdDynami__3214EC07014A7602");

            entity.HasIndex(e => e.AdId, "IX_AdDynamicValues_AdId");

            entity.HasIndex(e => e.FieldId, "IX_AdDynamicValues_FieldId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Ad).WithMany(p => p.AdDynamicValues)
                .HasForeignKey(d => d.AdId)
                .HasConstraintName("FK__AdDynamicV__AdId__2A164134");

            entity.HasOne(d => d.Field).WithMany(p => p.AdDynamicValues)
                .HasForeignKey(d => d.FieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdDynamic__Field__2B0A656D");
        });

        modelBuilder.Entity<AdImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AdImages__3214EC07F1A1223A");

            entity.HasIndex(e => e.AdId, "IX_AdImages_AdId");

            entity.HasIndex(e => e.IsPrimary, "IX_AdImages_IsPrimary");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Ad).WithMany(p => p.AdImages)
                .HasForeignKey(d => d.AdId)
                .HasConstraintName("FK__AdImages__AdId__47DBAE45");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC073EF60848");

            entity.HasIndex(e => e.ParentId, "IX_Categories_ParentId");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Categorie__Paren__3E52440B");
        });

        modelBuilder.Entity<DynamicField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DynamicF__3214EC07E7F6B5B5");

            entity.HasIndex(e => e.CategoryId, "IX_DynamicFields_CategoryId");

            entity.Property(e => e.FieldType).HasMaxLength(50);
            entity.Property(e => e.IsRequired).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.DynamicFields)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__DynamicFi__Categ__6FE99F9F");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Favorite__3214EC0710930395");

            entity.HasIndex(e => e.AdId, "IX_Favorites_AdId");

            entity.HasIndex(e => new { e.UserId, e.AdId }, "UQ__Favorite__E09BC1177C154E0A").IsUnique();

            entity.HasOne(d => d.Ad).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.AdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__AdId__5535A963");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Favorites__UserI__5441852A");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07E435F934");

            entity.HasIndex(e => e.AdId, "IX_Messages_AdId");

            entity.HasIndex(e => e.ReceiverId, "IX_Messages_ReceiverId");

            entity.HasIndex(e => e.SenderId, "IX_Messages_SenderId");

            entity.HasIndex(e => e.SentAt, "IX_Messages_SentAt").IsDescending();

            entity.Property(e => e.AttachmentType).HasMaxLength(50);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Ad).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AdId)
                .HasConstraintName("FK__Messages__AdId__4F7CD00D");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Receiv__4E88ABD4");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Sender__4D94879B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076C6B50C0");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E40A53B398").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534045CDDF4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.Otp).HasMaxLength(6);
            entity.Property(e => e.OtpAttempts).HasDefaultValue(0);
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime");
            entity.Property(e => e.OtpSentAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Buyer");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserAdActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserAdAc__3214EC07F20028FD");

            entity.HasIndex(e => e.AdId, "IX_UserAdActivities_Ad");

            entity.HasIndex(e => new { e.UserId, e.AdId, e.ActionType }, "IX_UserAdActivities_Dedup");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_UserAdActivities_User").IsDescending(false, true);

            entity.Property(e => e.ActionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Ad).WithMany(p => p.UserAdActivities)
                .HasForeignKey(d => d.AdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserActivity_Ad");

            entity.HasOne(d => d.User).WithMany(p => p.UserAdActivities)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserActivity_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
