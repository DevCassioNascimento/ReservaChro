// ReservaChro\Mobile\ReservaChro.Mobile\Views\TiDashboardPage.xaml.cs
using System.Collections.ObjectModel;

namespace ReservaChro.Mobile.Views;

public partial class TiDashboardPage : ContentPage
{
    private readonly Guid _schoolId;
    private static readonly Guid SchoolIdDiadema = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Diadema
    private static readonly Guid SchoolIdSbc = Guid.Parse("cea0f35d-7b03-44c2-b365-bc59cda6c073"); // São Bernardo do Campo

    private static readonly Guid SchoolIdEaa = Guid.Parse("b10550e8-a08e-4647-a44d-c7635a44c240"); // Americanopolis 

    private static readonly Guid SchoolIdCacia = Guid.Parse("0f1e3953-40ca-4a8b-95ad-e2f06fdd3d83"); // Cidade Ademar 

    private static readonly Guid SchoolIdCadg = Guid.Parse("5699b934-93e3-4419-91ff-513f3d645d06"); // Guaruja

    private static readonly Guid SchoolIdCai = Guid.Parse("9a529bf0-e491-48f5-8453-e569e668f856"); // Interlagos

    private static readonly Guid SchoolIdCaju = Guid.Parse("57764f1f-0a29-4969-a5a8-5a7106fef8c1"); // Jardim Utinga

    private static readonly Guid SchoolIdCam = Guid.Parse("4956125d-d1e6-49bf-a15a-870c3c43dfcb"); // Maua

    private static readonly Guid SchoolIdCap = Guid.Parse("b33dd6fb-e1d0-4f51-ac99-4b5a20d8f8cf"); // Pedreira

    private static readonly Guid SchoolIdCapg = Guid.Parse("102308b1-5588-4704-a83d-b25dd810fbeb");// Praia Grande

    private static readonly Guid SchoolIdCarr = Guid.Parse("2fd8108d-3bba-4234-b993-7ef2aeae9b99"); // Rudge Ramos

    private static readonly Guid SchoolIdCasa = Guid.Parse("00b01fa8-b32f-45d7-9434-ad519e921992"); // Santo André

    private static readonly Guid SchoolIdCas = Guid.Parse("e8c26a70-1eb6-419d-a9b5-3b6a16abcbc0"); // Santos

    private static readonly Guid SchoolIdCascs = Guid.Parse("985c5eb2-4268-4ed6-bcd9-e2726e36fd91"); // São Caetano do Sul
    private readonly string _name;
    private readonly string _role;

    public ObservableCollection<TiBookingVm> Bookings { get; } = new();

    // ✅ Construtor REAL: igual o padrão que você quer (recebe escola vinculada)
    public TiDashboardPage(string name, string role, Guid schoolId)
    {
        InitializeComponent();
        BindingContext = this;

        _name = string.IsNullOrWhiteSpace(name) ? "Usuário" : name.Trim();
        _role = string.IsNullOrWhiteSpace(role) ? "TI" : role.Trim();
        _schoolId = schoolId;

        ApplyHeader();
        ApplySchoolName();

        // Mock inicial (só pra tela nascer igual ao Figma)
        SeedMock();

        // Começa na aba Reservas
        SetTab("reservas");
    }

    // ✅ Mantém compatibilidade caso o XAML Previewer/Designer use o construtor vazio
    public TiDashboardPage() : this("Usuário", "TI", Guid.Empty) { }

    private void ApplyHeader()
    {
        // Nome e perfil no topo
        UserNameLabel.Text = _name;

        // Padroniza o texto do perfil
        if (_role.Equals("TI", StringComparison.OrdinalIgnoreCase) ||
            _role.Equals("Profissional de TI", StringComparison.OrdinalIgnoreCase))
        {
            UserRoleLabel.Text = "Profissional de TI";
        }
        else
        {
            UserRoleLabel.Text = _role;
        }
    }

    private void ApplySchoolName()
    {
        // ✅ Igual ao seu fluxo atual: mapeamento local por SchoolId
        // (sem chamar API)
        SchoolNameLabel.Text = MapSchoolName(_schoolId);
    }

    private static string MapSchoolName(Guid schoolId)
    {
        if (schoolId == Guid.Empty)
            return "Escola não identificada";

        // ✅ Mapeamento REAL usando os GUIDs que você já declarou
        if (schoolId == SchoolIdDiadema) return "Colégio Adventista de Diadema";
        if (schoolId == SchoolIdSbc) return "Colégio Adventista de São Bernardo do Campo";
        if (schoolId == SchoolIdEaa) return "Colégio Adventista de Americanópolis";
        if (schoolId == SchoolIdCacia) return "Colégio Adventista Cidade Ademar";
        if (schoolId == SchoolIdCadg) return "Colégio Adventista do Guarujá";
        if (schoolId == SchoolIdCai) return "Colégio Adventista de Interlagos";
        if (schoolId == SchoolIdCaju) return "Colégio Adventista Jardim Utinga";
        if (schoolId == SchoolIdCam) return "Colégio Adventista de Mauá";
        if (schoolId == SchoolIdCap) return "Colégio Adventista da Pedreira";
        if (schoolId == SchoolIdCapg) return "Colégio Adventista da Praia Grande";
        if (schoolId == SchoolIdCarr) return "Colégio Adventista de Rudge Ramos";
        if (schoolId == SchoolIdCasa) return "Colégio Adventista de Santo André";
        if (schoolId == SchoolIdCas) return "Colégio Adventista de Santos";
        if (schoolId == SchoolIdCascs) return "Colégio Adventista de São Caetano do Sul";

        return $"Escola não mapeada (SchoolId: {schoolId})";
    }

    private void SeedMock()
    {
        Bookings.Clear();

        Bookings.Add(new TiBookingVm
        {
            Id = "1",
            ProfessorName = "Professor Silva",
            DateText = "19/01/2026",
            Time = "08:00 - 09:00",
            QuantityText = "10 unidades",
            StatusText = "Pendente",
            StatusColor = Color.FromArgb("#7a5b16"),
            ShowConfirm = true,
            ShowRecusar = true
        });

        Bookings.Add(new TiBookingVm
        {
            Id = "2",
            ProfessorName = "Professora Maria",
            DateText = "19/01/2026",
            Time = "10:00 - 11:00",
            QuantityText = "15 unidades",
            StatusText = "Pendente",
            StatusColor = Color.FromArgb("#7a5b16"),
            ShowConfirm = true,
            ShowRecusar = true
        });

        Bookings.Add(new TiBookingVm
        {
            Id = "3",
            ProfessorName = "Professor João",
            DateText = "18/01/2026",
            Time = "14:00 - 15:00",
            QuantityText = "8 unidades",
            StatusText = "Em uso",
            StatusColor = Color.FromArgb("#1f6b4a"),
            ShowDevolucao = true
        });

        Bookings.Add(new TiBookingVm
        {
            Id = "4",
            ProfessorName = "Professora Ana",
            DateText = "18/01/2026",
            Time = "09:00 - 10:00",
            QuantityText = "12 unidades",
            StatusText = "Confirmada",
            StatusColor = Color.FromArgb("#1c4f86"),
            ShowIniciarUso = true
        });

        // Stats mock (depois você vai alimentar pela API, filtrando por _schoolId)
        PendentesValue.Text = "2";
        EmUsoValue.Text = "1";
        ConfirmadasValue.Text = "1";
        DisponivelValue.Text = "32";
        EstoqueTotalValue.Text = "40";
    }

    // ===== Tabs =====
    private void OnTabReservasClicked(object sender, EventArgs e) => SetTab("reservas");
    private void OnTabAgendaClicked(object sender, EventArgs e) => SetTab("agenda");
    private void OnTabEstoqueClicked(object sender, EventArgs e) => SetTab("estoque");

    private void SetTab(string tab)
    {
        ReservasPanel.IsVisible = tab == "reservas";
        AgendaPanel.IsVisible = tab == "agenda";
        EstoquePanel.IsVisible = tab == "estoque";

        SetTabButton(TabReservasBtn, tab == "reservas");
        SetTabButton(TabAgendaBtn, tab == "agenda");
        SetTabButton(TabEstoqueBtn, tab == "estoque");
    }

    private static void SetTabButton(Button btn, bool active)
    {
        btn.BackgroundColor = active ? Color.FromArgb("#1b63ff") : Colors.Transparent;
        btn.TextColor = active ? Colors.White : Color.FromArgb("#a8c4ff");
    }

    // ===== Actions (placeholders) =====
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Logout", "Aqui vamos implementar o logout real.", "OK");
    }

    private async void OnConfirmarClicked(object sender, EventArgs e)
    {
        var id = (sender as Button)?.CommandParameter?.ToString();
        await DisplayAlert("Confirmar", $"Confirmar reserva {id}", "OK");
    }

    private async void OnRecusarClicked(object sender, EventArgs e)
    {
        var id = (sender as Button)?.CommandParameter?.ToString();
        await DisplayAlert("Recusar", $"Recusar reserva {id}", "OK");
    }

    private async void OnConfirmarDevolucaoClicked(object sender, EventArgs e)
    {
        var id = (sender as Button)?.CommandParameter?.ToString();
        await DisplayAlert("Devolução", $"Confirmar devolução {id}", "OK");
    }

    private async void OnIniciarUsoClicked(object sender, EventArgs e)
    {
        var id = (sender as Button)?.CommandParameter?.ToString();
        await DisplayAlert("Iniciar Uso", $"Iniciar uso {id}", "OK");
    }

    private async void OnAtualizarEstoqueClicked(object sender, EventArgs e)
    {
        // Placeholder: aqui depois vai chamar endpoint /stock/update usando _schoolId
        await DisplayAlert("Estoque", $"Novo estoque: {NovoEstoqueEntry.Text}\nSchoolId: {_schoolId}", "OK");
    }
}

public class TiBookingVm
{
    public string Id { get; set; } = "";
    public string ProfessorName { get; set; } = "";
    public string DateText { get; set; } = "";
    public string Time { get; set; } = "";
    public string QuantityText { get; set; } = "";

    public string StatusText { get; set; } = "";
    public Color StatusColor { get; set; } = Colors.Gray;

    public bool ShowConfirm { get; set; }
    public bool ShowRecusar { get; set; }
    public bool ShowIniciarUso { get; set; }
    public bool ShowDevolucao { get; set; }
}
