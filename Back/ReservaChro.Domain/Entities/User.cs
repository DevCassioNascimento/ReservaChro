// Back/ReservaChro.Domain/Entities/User.cs
using ReservaChro.Domain.Enums;

namespace ReservaChro.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    // Para JWT + login: vamos armazenar hash e salt (não senha em texto)
    public string PasswordHash { get; private set; } = string.Empty;

    public string PasswordSalt { get; private set; } = string.Empty;

    public Role Role { get; private set; }

    // Isolamento por unidade:
    // Admin global pode ter SchoolId = null.
    // TI/Professor obrigatoriamente vinculados a uma escola.
    public Guid? SchoolId { get; private set; }

    // EF Core
    private User() { }

    public User(string name, string email, Role role, Guid? schoolId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("User name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("User email is required.", nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role;

        ValidateRoleSchoolConstraint(role, schoolId);
        SchoolId = schoolId;
    }

    public void SetPassword(string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(passwordSalt)) throw new ArgumentException("Password salt is required.", nameof(passwordSalt));

        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    private static void ValidateRoleSchoolConstraint(Role role, Guid? schoolId)
    {
        var isAdmin = role == Role.Admin;

        if (isAdmin && schoolId is not null)
            throw new InvalidOperationException("Admin must be global (SchoolId must be null).");

        if (!isAdmin && schoolId is null)
            throw new InvalidOperationException("Non-admin users must be linked to a school (SchoolId is required).");
    }
}
