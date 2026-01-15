// Back/ReservaChro.Domain/Entities/School.cs
namespace ReservaChro.Domain.Entities;

public sealed class School
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    // Opcional mas útil: identificador curto (ex: "APSe-ABC", "SANTOS-01")
    public string Code { get; private set; } = string.Empty;

    // EF Core
    private School() { }

    public School(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("School name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("School code is required.", nameof(code));

        Name = name.Trim();
        Code = code.Trim();
    }
}
