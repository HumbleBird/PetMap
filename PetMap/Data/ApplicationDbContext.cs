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
    public DbSet<PlaceView> PlaceViews => Set<PlaceView>();
    public DbSet<PetCourse> PetCourses => Set<PetCourse>();
    public DbSet<PetCourseStop> PetCourseStops => Set<PetCourseStop>();
    public DbSet<PetCourseLike> PetCourseLikes => Set<PetCourseLike>();
    public DbSet<PetCourseFavorite> PetCourseFavorites => Set<PetCourseFavorite>();
    public DbSet<PlaceCollection> PlaceCollections => Set<PlaceCollection>();
    public DbSet<PlaceCollectionItem> PlaceCollectionItems => Set<PlaceCollectionItem>();

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
            entity.Property(favorite => favorite.FolderName).HasMaxLength(80);
            entity.Property(favorite => favorite.Memo).HasMaxLength(500);
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

        modelBuilder.Entity<PlaceView>(entity =>
        {
            entity.HasIndex(view => new { view.PlaceId, view.ViewedAtUtc });
            entity.Property(view => view.UserId).HasMaxLength(450);
            entity.Property(view => view.ViewedAtUtc).HasColumnType("datetime2");
            entity.HasOne<Place>().WithMany().HasForeignKey(view => view.PlaceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(view => view.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PetCourse>(entity =>
        {
            entity.Property(course => course.UserId).HasMaxLength(450).IsRequired();
            entity.Property(course => course.Title).HasMaxLength(100).IsRequired();
            entity.Property(course => course.Description).HasMaxLength(2000);
            entity.Property(course => course.Category).HasMaxLength(200);
            entity.Property(course => course.City).HasMaxLength(80);
            entity.Property(course => course.District).HasMaxLength(80);
            entity.Property(course => course.Neighborhood).HasMaxLength(80);
            entity.Property(course => course.CreatedAtUtc).HasColumnType("datetime2");
            entity.HasIndex(course => new { course.IsPublic, course.CreatedAtUtc });
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(course => course.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PetCourseStop>(entity =>
        {
            entity.Property(stop => stop.Memo).HasMaxLength(500);
            entity.HasIndex(stop => new { stop.CourseId, stop.SortOrder });
            entity.HasIndex(stop => new { stop.CourseId, stop.PlaceId }).IsUnique();
            entity.HasOne<PetCourse>().WithMany(course => course.Stops).HasForeignKey(stop => stop.CourseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Place>().WithMany().HasForeignKey(stop => stop.PlaceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PetCourseLike>(entity =>
        {
            entity.HasKey(like => like.Id);
            entity.HasIndex(like => new { like.CourseId, like.UserId }).IsUnique();
            entity.Property(like => like.UserId).HasMaxLength(450).IsRequired();
            entity.Property(like => like.CreatedAtUtc).HasColumnType("datetime2");
            // User 삭제는 사용자의 반응을 정리하지만, 코스 삭제는 앱에서 반응을 먼저 지워 다중 Cascade 경로를 막는다.
            entity.HasOne<PetCourse>().WithMany().HasForeignKey(like => like.CourseId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(like => like.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PetCourseFavorite>(entity =>
        {
            entity.HasKey(favorite => favorite.Id);
            entity.HasIndex(favorite => new { favorite.CourseId, favorite.UserId }).IsUnique();
            entity.Property(favorite => favorite.UserId).HasMaxLength(450).IsRequired();
            entity.Property(favorite => favorite.CreatedAtUtc).HasColumnType("datetime2");
            entity.HasOne<PetCourse>().WithMany().HasForeignKey(favorite => favorite.CourseId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(favorite => favorite.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaceCollection>(entity =>
        {
            entity.Property(collection => collection.UserId).HasMaxLength(450).IsRequired();
            entity.Property(collection => collection.Name).HasMaxLength(80).IsRequired();
            entity.Property(collection => collection.City).HasMaxLength(80);
            entity.Property(collection => collection.District).HasMaxLength(80);
            entity.Property(collection => collection.Neighborhood).HasMaxLength(80);
            entity.Property(collection => collection.Memo).HasMaxLength(1000);
            entity.Property(collection => collection.CreatedAtUtc).HasColumnType("datetime2");
            entity.HasIndex(collection => new { collection.UserId, collection.Name }).IsUnique();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(collection => collection.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaceCollectionItem>(entity =>
        {
            entity.HasKey(item => new { item.CollectionId, item.PlaceId });
            entity.Property(item => item.Memo).HasMaxLength(500);
            entity.Property(item => item.CreatedAtUtc).HasColumnType("datetime2");
            entity.HasOne<PlaceCollection>().WithMany().HasForeignKey(item => item.CollectionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Place>().WithMany().HasForeignKey(item => item.PlaceId).OnDelete(DeleteBehavior.Cascade);
        });
    }

}
