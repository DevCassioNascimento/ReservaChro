using Microsoft.EntityFrameworkCore;
using ReservaChro.Domain.Entities;
using ReservaChro.Domain.Enums;

namespace ReservaChro.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<School> Schools => Set<School>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.PasswordSalt)
                .IsRequired();

            entity.Property(u => u.Role)
                .IsRequired();

            entity.Property(u => u.SchoolId)
                .IsRequired(false);
        });

        // SCHOOL
        modelBuilder.Entity<School>(entity =>
        {
            entity.ToTable("Schools");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(s => s.Code)
                .IsUnique();
        });

        // SEED (determinístico para ambiente de desenvolvimento)
        var schoolId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<School>().HasData(new
        {
            Id = schoolId,
            Name = "Escola Modelo",
            Code = "ESC-001"
        });

        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Admin Global",
                Email = "admin",
                PasswordHash = "123456",
                PasswordSalt = "dev",
                Role = Role.Admin,
                SchoolId = (Guid?)null
            },
            new
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "TI Local",
                Email = "ti",
                PasswordHash = "123456",
                PasswordSalt = "dev",
                Role = Role.TI,
                SchoolId = (Guid?)schoolId
            },
            new
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Professor Local",
                Email = "professor",
                PasswordHash = "123456",
                PasswordSalt = "dev",
                Role = Role.Professor,
                SchoolId = (Guid?)schoolId
            }
        );
    }
}
