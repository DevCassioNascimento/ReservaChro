// Back/ReservaChro.Domain/Entities/School.cs
namespace ReservaChro.Domain.Entities;

public sealed class School
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    // Opcional mas útil: identificador curto (ex: "APSe-ABC", "SANTOS-01")
    public string Code { get; private set; } = string.Empty;

    // Quantidade total de chromebooks no estoque da escola
    public int QuantidadeEstoque { get; private set; } = 0;

    // EF Core
    private School() { }

    public School(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("School name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("School code is required.", nameof(code));

        Name = name.Trim();
        Code = code.Trim();
        QuantidadeEstoque = 0;
    }

    public void AtualizarQuantidadeEstoque(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("A quantidade de estoque não pode ser negativa.", nameof(quantidade));
        
        QuantidadeEstoque = quantidade;
    }
}
