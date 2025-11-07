using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Data;

public class GoalTrackerAppDbContext : DbContext
{
    public GoalTrackerAppDbContext()
    {
    }

    public GoalTrackerAppDbContext(DbContextOptions<GoalTrackerAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Goal> Goals { get; set; } = null!;
    public DbSet<GoalCategory> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(60);
            entity.Property(e => e.Firstname).HasMaxLength(255);
            entity.Property(e => e.Lastname).HasMaxLength(255);
            
            entity.Property(e => e.UserRole)
                .HasMaxLength(20)
                .HasConversion<string>();

            entity.Property(e => e.InsertedAt)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ModifiedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();
        });

         modelBuilder.Entity<Goal>(entity =>
         {
             entity.ToTable("Goals");
             entity.HasKey(e => e.Id);
             
             entity.Property(e => e.Title)
                 .IsRequired()
                 .HasMaxLength(255);
             
             entity.Property(e => e.Description)
                 .IsRequired(false)
                 .HasMaxLength(255);
             
             entity.Property(e => e.GoalStatus)
                 .HasMaxLength(20)
                 .HasConversion<string>();
             
             entity.Property(e => e.DueDate)
                 .IsRequired(false);
             
             entity.Property(e => e.InsertedAt)
                 .ValueGeneratedOnAdd()
                 .HasDefaultValueSql("GETUTCDATE()");
             
             entity.Property(e => e.ModifiedAt)
                 .ValueGeneratedOnAddOrUpdate()
                 .HasDefaultValueSql("GETUTCDATE()");

             entity.HasOne(e => e.User)
                 .WithMany(u => u.Goals)
                 .HasForeignKey(e => e.UserId) 
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Cascade);
             
             entity.HasOne(g => g.GoalCategory)
                 .WithMany(c => c.Goals)
                 .HasForeignKey(g => g.GoalCategoryId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull); 
             
             entity.HasIndex(g => g.UserId, "IX_Goals_UserId");
             
             entity.HasIndex(g => g.GoalStatus, "IX_Goals_GoalStatus");
         });
         
         modelBuilder.Entity<GoalCategory>(entity =>
         {
             entity.ToTable("GoalCategories");
             entity.HasKey(e => e.Id);

             entity.Property(e => e.Name)
                 .IsRequired()
                 .HasMaxLength(100);
             
             entity.Property(e => e.InsertedAt)
                 .ValueGeneratedOnAdd()
                 .HasDefaultValueSql("GETUTCDATE()");

             entity.Property(e => e.ModifiedAt)
                 .ValueGeneratedOnAddOrUpdate()
                 .HasDefaultValueSql("GETUTCDATE()");
             
             entity.HasOne(g => g.User)
                 .WithMany(u => u.GoalCategories)
                 .HasForeignKey(g => g.UserId)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.NoAction); 
             
             entity.HasIndex(g => g.UserId, "IX_GoalCategories_UserId");
             
             entity.HasIndex(u => new { u.UserId, u.Name }, "IX_GoalCategories_UserId_Name")
                 .IsUnique();
         });
    }
}
