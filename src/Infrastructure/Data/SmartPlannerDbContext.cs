using Microsoft.EntityFrameworkCore;
using SmartPlanner.Domain.Entities;

namespace SmartPlanner.Infrastructure.Data
{
    public class SmartPlannerDbContext : DbContext
    {
        public SmartPlannerDbContext(DbContextOptions<SmartPlannerDbContext> options) : base(options)
        {
        }

        public DbSet<StudentAccount> StudentAccounts { get; set; } = null!;
        public DbSet<Domain.Entities.Task> Tasks { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure StudentAccount
            modelBuilder.Entity<StudentAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Email).HasMaxLength(320).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // Configure Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Foreign key to StudentAccount
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Subjects)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                // Unique constraint: subject name per student
                entity.HasIndex(e => new { e.StudentId, e.Name }).IsUnique();
            });

            // Configure Task
            modelBuilder.Entity<Domain.Entities.Task>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Deadline).IsRequired();
                entity.Property(e => e.IsDone).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Foreign key to StudentAccount
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                // Foreign key to Subject (optional)
                entity.HasOne(e => e.Subject)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                // Index for performance
                entity.HasIndex(e => new { e.StudentId, e.Deadline });
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedAt") != null)
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                
                if (entry.Property("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
