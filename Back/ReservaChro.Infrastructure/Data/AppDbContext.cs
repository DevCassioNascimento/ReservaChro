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

    // ✅ Padronização: plural é mais seguro no código.
    // Mantemos a tabela como "Chromestoque" (já está assim no banco/migration).
    public DbSet<Chromestoque> Chromestoques => Set<Chromestoque>();

    // ✅ Compatibilidade (caso algum código antigo ainda use Chromestoque)
    public DbSet<Chromestoque> Chromestoque => Set<Chromestoque>();

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

        // CHROMESTOQUE
        modelBuilder.Entity<Chromestoque>(entity =>
        {
            entity.ToTable("Chromestoque");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.NomeMaquina)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.NumeroSerie)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(c => c.NumeroSerie)
                .IsUnique();

            entity.Property(c => c.Modelo)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.DataAquisicao)
                .IsRequired();

            entity.Property(c => c.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(c => c.SchoolId)
                .IsRequired();

            // ✅ FK explícita para garantir integridade e evitar bugs silenciosos
            entity.HasOne<School>()
                .WithMany()
                .HasForeignKey(c => c.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
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
