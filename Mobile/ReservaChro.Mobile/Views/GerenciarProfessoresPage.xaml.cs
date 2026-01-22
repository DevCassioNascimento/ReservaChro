using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReservaChro.Mobile.Views;

public partial class GerenciarProfessoresPage : ContentPage
{
    private readonly string _apiBaseUrl;
    private string? _token;
    private readonly ObservableCollection<ProfessorVm> _professores = new();

    public GerenciarProfessoresPage(string? token = null)
    {
        InitializeComponent();
        _token = token;
        _apiBaseUrl = ResolveApiBaseUrl();
        ProfessoresCollectionView.ItemsSource = _professores;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfessores();
    }

    private static string ResolveApiBaseUrl()
    {
        // Emulador Android -> 10.0.2.2
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5193";

        // Windows / iOS / outros em debug normalmente acessam localhost
        return "http://localhost:5193";
    }

    private async Task EnsureTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            _token = await SecureStorage.GetAsync("auth_token");
        }
    }

    private async Task LoadProfessores()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            EmptyStateLabel.IsVisible = false;

            await EnsureTokenAsync();

            if (string.IsNullOrWhiteSpace(_token))
            {
                await DisplayAlert("Erro", "Token não encontrado. Faça login novamente.", "OK");
                await Navigation.PopAsync();
                return;
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.GetAsync($"{_apiBaseUrl}/auth/professores");
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] GET /auth/professores -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (!response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao carregar professores.\n\n{mensagemErro}", "OK");
                return;
            }

            // Deserializar resposta
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<ProfessorResponseDto>? professores = null;

            try
            {
                professores = JsonSerializer.Deserialize<List<ProfessorResponseDto>>(body, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TI] Erro ao deserializar: {ex}");
            }

            if (professores == null)
            {
                professores = new List<ProfessorResponseDto>();
            }

            System.Diagnostics.Debug.WriteLine($"[TI] Total de professores deserializados: {professores.Count}");

            // Limpar e atualizar lista
            _professores.Clear();

            foreach (var professor in professores)
            {
                if (professor.Id == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[TI] Professor com ID vazio ignorado");
                    continue;
                }

                _professores.Add(new ProfessorVm
                {
                    Id = professor.Id.ToString(),
                    Name = string.IsNullOrWhiteSpace(professor.Name) ? "Professor" : professor.Name,
                    Email = string.IsNullOrWhiteSpace(professor.Email) ? "Sem e-mail" : professor.Email
                });
            }

            // Mostrar empty state se não houver professores
            if (_professores.Count == 0)
            {
                EmptyStateLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao carregar professores: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção LoadProfessores: {ex}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAdicionarProfessorClicked(object sender, EventArgs e)
    {
        await EnsureTokenAsync();

        if (string.IsNullOrWhiteSpace(_token))
        {
            await DisplayAlert("Erro", "Token não encontrado. Faça login novamente.", "OK");
            return;
        }

        // Solicitar nome do professor
        var nome = await DisplayPromptAsync(
            "Adicionar Professor",
            "Digite o nome do professor:",
            "OK",
            "Cancelar",
            "",
            -1,
            Keyboard.Default,
            "");

        if (string.IsNullOrWhiteSpace(nome))
            return;

        // Solicitar e-mail do professor
        var email = await DisplayPromptAsync(
            "Adicionar Professor",
            "Digite o e-mail do professor:",
            "OK",
            "Cancelar",
            "",
            -1,
            Keyboard.Email,
            "");

        if (string.IsNullOrWhiteSpace(email))
            return;

        // Validar formato básico de e-mail
        if (!email.Contains("@") || !email.Contains("."))
        {
            await DisplayAlert("Erro", "E-mail inválido.", "OK");
            return;
        }

        // Enviar requisição para criar professor
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var request = new
            {
                email = email.Trim(),
                name = nome.Trim()
            };

            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/auth/professor", request);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] POST /auth/professor -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var senhaPadrao = doc.RootElement.TryGetProperty("senhaPadrao", out var senhaElement)
                        ? senhaElement.GetString()
                        : "123456";

                    await DisplayAlert(
                        "Sucesso",
                        $"✅ Professor criado com sucesso!\n\nE-mail: {email}\nSenha padrão: {senhaPadrao}\n\nO professor pode alterar a senha após fazer login.",
                        "OK");
                }
                catch
                {
                    await DisplayAlert("Sucesso", "✅ Professor criado com sucesso!", "OK");
                }

                // Recarregar lista
                await LoadProfessores();
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                        ? msgElement.GetString()
                        : $"Erro {(int)response.StatusCode}";

                    await DisplayAlert("Erro", $"❌ Falha ao criar professor.\n\n{mensagemErro}", "OK");
                }
                catch
                {
                    await DisplayAlert("Erro", $"❌ Falha ao criar professor.\n\nErro {(int)response.StatusCode}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao criar professor: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnAdicionarProfessorClicked: {ex}");
        }
    }

    private async void OnExcluirProfessorClicked(object sender, EventArgs e)
    {
        var idString = (sender as Button)?.CommandParameter?.ToString();
        if (string.IsNullOrWhiteSpace(idString) || !Guid.TryParse(idString, out var professorId))
        {
            await DisplayAlert("Erro", "ID do professor inválido.", "OK");
            return;
        }

        // Buscar nome do professor para exibir na confirmação
        var professor = _professores.FirstOrDefault(p => p.Id == idString);
        var nomeProfessor = professor?.Name ?? "este professor";

        // Confirmar ação
        var confirmar = await DisplayAlert(
            "Confirmar Exclusão",
            $"Tem certeza que deseja excluir {nomeProfessor}?\n\nEsta ação não pode ser desfeita.",
            "Sim, Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await EnsureTokenAsync();

            if (string.IsNullOrWhiteSpace(_token))
            {
                await DisplayAlert("Erro", "Token não encontrado. Faça login novamente.", "OK");
                return;
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.DeleteAsync($"{_apiBaseUrl}/auth/professor/{professorId}");
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] DELETE /auth/professor/{professorId} -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "✅ Professor excluído com sucesso!", "OK");
                // Recarregar lista
                await LoadProfessores();
            }
            else
            {
                using var doc = JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao excluir professor.\n\n{mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao excluir professor: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnExcluirProfessorClicked: {ex}");
        }
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadProfessores();
    }

    public ObservableCollection<ProfessorVm> Professores => _professores;
}

// ViewModel para exibição
public class ProfessorVm
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// DTO para deserialização
public class ProfessorResponseDto
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public Guid Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
