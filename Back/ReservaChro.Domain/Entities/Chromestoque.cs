namespace ReservaChro.Domain.Entities;

public sealed class Chromestoque
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string NomeMaquina { get; private set; } = string.Empty;

    public string NumeroSerie { get; private set; } = string.Empty;

    public string Modelo { get; private set; } = string.Empty;

    public bool Ativo { get; private set; } = true;

    public DateTime DataAquisicao { get; private set; }

    public Guid SchoolId { get; private set; }

    // EF Core
    private Chromestoque() { }

    public Chromestoque(string nomeMaquina, string numeroSerie, string modelo, DateTime dataAquisicao, Guid schoolId)
    {
        if (string.IsNullOrWhiteSpace(nomeMaquina)) throw new ArgumentException("Nome da máquina é obrigatório.", nameof(nomeMaquina));
        if (string.IsNullOrWhiteSpace(numeroSerie)) throw new ArgumentException("Número de série é obrigatório.", nameof(numeroSerie));
        if (string.IsNullOrWhiteSpace(modelo)) throw new ArgumentException("Modelo é obrigatório.", nameof(modelo));
        if (schoolId == Guid.Empty) throw new ArgumentException("SchoolId é obrigatório.", nameof(schoolId));

        NomeMaquina = nomeMaquina.Trim();
        NumeroSerie = numeroSerie.Trim();
        Modelo = modelo.Trim();
        DataAquisicao = dataAquisicao;
        SchoolId = schoolId;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void AtualizarNomeMaquina(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new ArgumentException("Nome da máquina não pode estar vazio.", nameof(novoNome));
        NomeMaquina = novoNome.Trim();
    }

    public void AtualizarModelo(string novoModelo)
    {
        if (string.IsNullOrWhiteSpace(novoModelo))
            throw new ArgumentException("Modelo não pode estar vazio.", nameof(novoModelo));
        Modelo = novoModelo.Trim();
    }
}
