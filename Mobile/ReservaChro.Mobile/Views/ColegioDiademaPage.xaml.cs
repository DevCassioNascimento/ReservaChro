namespace ReservaChro.Mobile.Views;

public partial class ColegioDiademaPage : ContentPage
{
    public ColegioDiademaPage(string userName = "Usuário", string roleName = "Perfil")
    {
        InitializeComponent();

        UserNameLabel.Text = userName;
        UserRoleLabel.Text = roleName;

        // Horários exemplo (depois vamos buscar da API)
        HorarioPicker.ItemsSource = new List<string>
        {
            "07:00", "08:00", "09:00", "10:00",
            "11:00", "13:00", "14:00", "15:00", "16:00"
        };
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

        if (qtd > 40)
        {
            await DisplayAlert("Limite", "Máximo de 40 Chromebooks por horário.", "OK");
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
