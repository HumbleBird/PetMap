using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetMap.Data.Entities;

namespace PetMap.Data;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<User> Users => Set<User>(); // Test
    public DbSet<Place> Places { get; set; }
    public DbSet<PlaceReport> PlaceReports { get; set; }

    public DbSet<PlaceImage> PlaceImages => Set<PlaceImage>();
    public DbSet<PlaceRating> PlaceRatings => Set<PlaceRating>();
    public DbSet<PlaceComment> PlaceComments => Set<PlaceComment>();
    public DbSet<PlaceFavorite> PlaceFavorites => Set<PlaceFavorite>();
    public DbSet<PlaceCommentLike> PlaceCommentLikes => Set<PlaceCommentLike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlaceRating>(entity =>
        {
            entity.HasIndex(rating => new { rating.PlaceId, rating.UserId }).IsUnique();
            entity.Property(rating => rating.UserId).HasMaxLength(450).IsRequired();
            entity.Property(rating => rating.UpdatedAtUtc).HasColumnType("datetime2");
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_PlaceRatings_Score_Range",
                "[Score] >= 1 AND [Score] <= 5"));

            entity.HasOne(rating => rating.Place)
                .WithMany(place => place.Ratings)
                .HasForeignKey(rating => rating.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(rating => rating.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaceComment>(entity =>
        {
            entity.Property(comment => comment.AuthorName).HasMaxLength(80).IsRequired();
            entity.Property(comment => comment.Content).HasMaxLength(2000).IsRequired();
            entity.Property(comment => comment.GuestPasswordHash).HasMaxLength(500);
            entity.Property(comment => comment.CreatedAt).HasColumnType("datetime2");
            entity.HasIndex(comment => new { comment.PlaceId, comment.CreatedAt });
            entity.HasOne<Place>().WithMany().HasForeignKey(comment => comment.PlaceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(comment => comment.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlaceFavorite>(entity =>
        {
            entity.HasKey(favorite => new { favorite.PlaceId, favorite.UserId });
            entity.Property(favorite => favorite.UserId).HasMaxLength(450).IsRequired();
            entity.Property(favorite => favorite.CreatedAt).HasColumnType("datetime2");
            entity.HasOne<Place>().WithMany().HasForeignKey(favorite => favorite.PlaceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(favorite => favorite.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaceCommentLike>(entity =>
        {
            entity.HasKey(like => new { like.CommentId, like.UserId });
            entity.Property(like => like.UserId).HasMaxLength(450).IsRequired();
            entity.Property(like => like.CreatedAt).HasColumnType("datetime2");
            entity.HasOne<PlaceComment>().WithMany().HasForeignKey(like => like.CommentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(like => like.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }

}
