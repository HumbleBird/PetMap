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

}
