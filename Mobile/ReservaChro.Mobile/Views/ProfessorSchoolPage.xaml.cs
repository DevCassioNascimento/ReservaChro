// Caminho: ReservaChro\Mobile\ReservaChro.Mobile\Views\ProfessorSchoolPage.xaml.cs
namespace ReservaChro.Mobile.Views;

public partial class ProfessorSchoolPage : ContentPage
{
    private readonly Guid _schoolId;

    // Se você já tem IDs fixos no app, mantenha aqui (ajuste depois para vir do backend se quiser)
    private static readonly Guid SchoolIdDiadema = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Diadema
    private static readonly Guid SchoolIdSbc = Guid.Parse("cea0f35d-7b03-44c2-b365-bc59cda6c073"); // São Bernardo do Campo

    private static readonly Guid SchoolIdEaa = Guid.Parse("b10550e8-a08e-4647-a44d-c7635a44c240"); // Americanopolis 

    private static readonly Guid SchoolIdCacia = Guid.Parse("0f1e3953-40ca-4a8b-95ad-e2f06fdd3d83"); // Cidade Ademar 

    private static readonly Guid SchoolIdCadg = Guid.Parse("5699b934-93e3-4419-91ff-513f3d645d06"); // Guaruja

    private static readonly Guid SchoolIdCai = Guid.Parse("9a529bf0-e491-48f5-8453-e569e668f856"); // Interlagos

    private static readonly Guid SchoolIdCaju = Guid.Parse("57764f1f-0a29-4969-a5a8-5a7106fef8c1"); // Jardim Utinga

    private static readonly Guid SchoolIdCam = Guid.Parse("4956125d-d1e6-49bf-a15a-870c3c43dfcb"); // Maua

    private static readonly Guid SchoolIdCap = Guid.Parse("b33dd6fb-e1d0-4f51-ac99-4b5a20d8f8cf"); // Pedreira

    private static readonly Guid SchoolIdCapg = Guid.Parse("c8672f36-04c1-4202-822d-5b24117845d0");// Praia Grande

    private static readonly Guid SchoolIdCarr = Guid.Parse("2fd8108d-3bba-4234-b993-7ef2aeae9b99"); // Rudge Ramos

    private static readonly Guid SchoolIdCasa = Guid.Parse("00b01fa8-b32f-45d7-9434-ad519e921992"); // Santo André

    private static readonly Guid SchoolIdCas = Guid.Parse("e8c26a70-1eb6-419d-a9b5-3b6a16abcbc0"); // Santos

    private static readonly Guid SchoolIdCascs = Guid.Parse("985c5eb2-4268-4ed6-bcd9-e2726e36fd91"); // São Caetano do Sul

    private sealed record SchoolUiConfig(string DisplayName, string LogoImage, string Title);

    private static readonly Dictionary<Guid, SchoolUiConfig> SchoolUiMap = new()
    {
        [SchoolIdDiadema] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Diadema",
            LogoImage: "colegiocad.png",
            Title: "Colégio Adventista de Diadema"
        ),
        [SchoolIdSbc] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de São Bernardo do Campo",
            LogoImage: "sbccolegio.png",
            Title: "Colégio Adventista de São Bernardo do Campo"
        ),

        [SchoolIdCadg] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Guarujá",
            LogoImage: "colegiocadg.png",
            Title: "Colégio Adventista de Guarujá"
        ),

        [SchoolIdCarr] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Rudge Ramos",
            LogoImage: "colegiocarr.png",
            Title: "Colégio Adventista de Rudge Ramos"
        ),

        [SchoolIdCaju] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Jardim Utinga",
            LogoImage: "colegiocaju.png",
            Title: "Colégio Adventista de Jardim Utinga"
        ),

        [SchoolIdCam] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Mauá",
            LogoImage: "mauacolegio.png",
            Title: "Colégio Adventista de Mauá"
        ),

        [SchoolIdCap] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Pedreira",
            LogoImage: "colegiocap.png",
            Title: "Colégio Adventista de Pedreira"
        ),

        [SchoolIdCapg] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Praia Grande",
            LogoImage: "colegiocapg.png",
            Title: "Colégio Adventista de Praia Grande"
        ),

        [SchoolIdCasa] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Santo André",
            LogoImage: "sacolegio.png",
            Title: "Colégio Adventista de Santo André"
        ),

        [SchoolIdCas] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Santos",
            LogoImage: "colegiocas.png",
            Title: "Colégio Adventista de Santos"
        ),

        [SchoolIdCascs] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de São Caetano do Sul",
            LogoImage: "colegiocsacs.png",
            Title: "Colégio Adventista de São Caetano do Sul"
        ),
        [SchoolIdCai] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Interlagos",
            LogoImage: "colegiocai.png",
            Title: "Colégio Adventista de Interlagos"
        ),
        [SchoolIdEaa] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Americanópolis",
            LogoImage: "colegioeaa.png",
            Title: "Colégio Adventista de Americanópolis"
        ),
        [SchoolIdCacia] = new SchoolUiConfig(
            DisplayName: "Colégio Adventista de Cidade Ademar",
            LogoImage: "colegiocacia.png",
            Title: "Colégio Adventista de Cidade Ademar"
        ),

    };

    // Construtor principal (novo)
    public ProfessorSchoolPage(Guid schoolId, string userName = "Usuário", string roleName = "Perfil")
    {
        InitializeComponent();

        _schoolId = schoolId;

        UserNameLabel.Text = userName;
        UserRoleLabel.Text = roleName;

        ApplySchoolUi(schoolId);

        // Horários exemplo (depois vamos buscar da API)
        HorarioPicker.ItemsSource = new List<string>
        {
            "07:00", "08:00", "09:00", "10:00",
            "11:00", "13:00", "14:00", "15:00", "16:00"
        };
    }

    // (Opcional) Construtor de compatibilidade, caso algum lugar ainda chame sem SchoolId
    public ProfessorSchoolPage(string userName = "Usuário", string roleName = "Perfil")
        : this(Guid.Empty, userName, roleName)
    {
    }

    private void ApplySchoolUi(Guid schoolId)
    {
        if (schoolId != Guid.Empty && SchoolUiMap.TryGetValue(schoolId, out var cfg))
        {
            Title = cfg.Title;
            SchoolNameLabel.Text = cfg.DisplayName;
            SchoolLogoImage.Source = cfg.LogoImage;
            return;
        }

        // Fallback (caso SchoolId ainda não esteja mapeado)
        Title = "Colégio";
        SchoolNameLabel.Text = "Colégio";
        // mantém a imagem default do XAML
    }

    private async void OnConfirmarClicked(object sender, EventArgs e)
    {
        // Placeholder: por enquanto só validação simples
        if (HorarioPicker.SelectedItem is null)
        {
            await DisplayAlert("Atenção", "Selecione um horário.", "OK");
            return;
        }

        if (!int.TryParse(QuantidadeEntry.Text, out var qtd) || qtd <= 0)
        {
            await DisplayAlert("Atenção", "Informe uma quantidade válida.", "OK");
            return;
        }

        // Mantive a mesma regra que você já tinha: limite “hardcoded”
        if (qtd > 40)
        {
            await DisplayAlert("Atenção", "Quantidade máxima permitida: 40.", "OK");
            return;
        }

        await DisplayAlert("OK", "Agendamento (simulado) confirmado.", "Fechar");
    }

    private async void OnMinhasReservasClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Em breve", "Tela de 'Minhas Reservas' será a próxima.", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Placeholder: depois vamos limpar token e voltar pro login
        await DisplayAlert("Logout", "Logout (simulado).", "OK");
    }
}
