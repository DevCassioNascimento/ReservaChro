using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ReservaChro.Mobile.Views;

public partial class ProfessorReservasPage : ContentPage
{
    private readonly string _apiBaseUrl;
    private string? _token;
    private readonly ObservableCollection<ProfessorReservaVm> _reservas = new();

    public ProfessorReservasPage(string? token = null)
    {
        InitializeComponent();
        _token = token;
        _apiBaseUrl = ResolveApiBaseUrl();
        ReservasCollectionView.ItemsSource = _reservas;
        BindingContext = this;
    }

    private static string ResolveApiBaseUrl()
    {
        // Emulador Android -> 10.0.2.2
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5193";

        // Windows / iOS / outros em debug normalmente acessam localhost
        return "http://localhost:5193";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReservas();
    }

    private async Task EnsureTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            _token = await SecureStorage.GetAsync("auth_token");
        }
    }

    private async Task LoadReservas()
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

            var response = await client.GetAsync($"{_apiBaseUrl}/reservas/minhas");
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[PROF] GET /reservas/minhas -> {(int)response.StatusCode} {response.StatusCode} | Body length: {body?.Length ?? 0} | Body: {body}");

            if (!response.IsSuccessStatusCode)
            {
                string mensagemErro = $"Erro {(int)response.StatusCode}";
                
                // Tentar extrair mensagem de erro do JSON, se houver
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("message", out var msgElement))
                        {
                            mensagemErro = msgElement.GetString() ?? mensagemErro;
                        }
                    }
                    catch
                    {
                        // Se não for JSON válido, usar o body como mensagem (pode ser HTML de erro)
                        mensagemErro = body.Length > 200 ? body.Substring(0, 200) + "..." : body;
                    }
                }

                await DisplayAlert("Erro", $"❌ Falha ao carregar reservas.\n\n{mensagemErro}", "OK");
                return;
            }

            // Verificar se o body está vazio
            if (string.IsNullOrWhiteSpace(body))
            {
                System.Diagnostics.Debug.WriteLine($"[PROF] Resposta vazia - retornando lista vazia");
                _reservas.Clear();
                EmptyStateLabel.IsVisible = true;
                return;
            }

            // Deserializar resposta
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<ReservaResponseDto>? reservas = null;

            // Tentar deserializar como array direto
            try
            {
                // Verificar se é JSON válido antes de deserializar
                if (body.TrimStart().StartsWith("[") || body.TrimStart().StartsWith("{"))
                {
                    reservas = JsonSerializer.Deserialize<List<ReservaResponseDto>>(body, options);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PROF] Resposta não é JSON válido: {body.Substring(0, Math.Min(100, body.Length))}");
                    reservas = new List<ReservaResponseDto>();
                }
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROF] Erro JSON ao deserializar direto: {ex.Message}");
                // Se falhar, tentar como objeto com propriedade 'data' ou 'reservas'
                try
                {
                    if (body.TrimStart().StartsWith("{"))
                    {
                        using var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            reservas = JsonSerializer.Deserialize<List<ReservaResponseDto>>(dataElement.GetRawText(), options);
                        }
                        else if (doc.RootElement.TryGetProperty("reservas", out var reservasElement))
                        {
                            reservas = JsonSerializer.Deserialize<List<ReservaResponseDto>>(reservasElement.GetRawText(), options);
                        }
                        else
                        {
                            reservas = new List<ReservaResponseDto>();
                        }
                    }
                    else
                    {
                        reservas = new List<ReservaResponseDto>();
                    }
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"[PROF] Erro ao deserializar com fallback: {ex2.Message}");
                    reservas = new List<ReservaResponseDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROF] Erro inesperado ao deserializar: {ex.Message}");
                reservas = new List<ReservaResponseDto>();
            }

            if (reservas == null)
            {
                reservas = new List<ReservaResponseDto>();
            }

            System.Diagnostics.Debug.WriteLine($"[PROF] Total de reservas deserializadas: {reservas?.Count ?? 0}");

            // Limpar e atualizar lista
            _reservas.Clear();

            // Se não houver reservas, mostrar estado vazio e retornar
            if (reservas == null || reservas.Count == 0)
            {
                EmptyStateLabel.IsVisible = true;
                PendentesLabel.Text = "0";
                ConfirmadasLabel.Text = "0";
                RecusadasLabel.Text = "0";
                return;
            }

            // Contadores
            int pendentes = 0, confirmadas = 0, recusadas = 0;

            foreach (var reserva in reservas)
            {
                // Determinar status text e color
                string statusText;
                Color statusColor;
                string dataText = reserva.DataCriacao.ToString("dd/MM/yyyy HH:mm");

                switch (reserva.Status)
                {
                    case 1: // Pendente
                        statusText = "⏳ Pendente";
                        statusColor = Color.FromArgb("#ffd93d");
                        pendentes++;
                        break;
                    case 2: // Confirmada
                        statusText = "✅ Confirmada";
                        statusColor = Color.FromArgb("#6bcf7f");
                        confirmadas++;
                        break;
                    case 3: // Recusada
                        statusText = "❌ Recusada";
                        statusColor = Color.FromArgb("#ff6b6b");
                        recusadas++;
                        break;
                    case 4: // EmUso
                        statusText = "🔄 Em Uso";
                        statusColor = Color.FromArgb("#4a9eff");
                        break;
                    case 5: // Concluida
                        statusText = "✔️ Concluída";
                        statusColor = Color.FromArgb("#6bcf7f");
                        break;
                    default:
                        statusText = "❓ Desconhecido";
                        statusColor = Colors.Gray;
                        break;
                }

                // Validar se os dados estão corretos antes de adicionar
                if (reserva.Id == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[PROF] Reserva com ID vazio ignorada");
                    continue;
                }

                // Formatar data corretamente (pode estar em UTC)
                var dataFormatada = reserva.DataReserva == default
                    ? "Data inválida"
                    : reserva.DataReserva.ToString("dd/MM/yyyy");

                // Formatar horário
                var horarioFormatado = reserva.HorarioInicio == default || reserva.HorarioFim == default
                    ? "Horário inválido"
                    : $"{reserva.HorarioInicio:hh\\:mm} - {reserva.HorarioFim:hh\\:mm}";

                // Validar quantidade
                var quantidadeTexto = reserva.Quantidade <= 0
                    ? "0 unidades"
                    : $"{reserva.Quantidade} unidades";

                _reservas.Add(new ProfessorReservaVm
                {
                    Id = reserva.Id.ToString(),
                    DateText = dataFormatada,
                    Time = horarioFormatado,
                    QuantityText = quantidadeTexto,
                    StatusText = statusText,
                    StatusColor = statusColor,
                    DataText = dataText
                });
            }

            // Atualizar contadores
            PendentesLabel.Text = pendentes.ToString();
            ConfirmadasLabel.Text = confirmadas.ToString();
            RecusadasLabel.Text = recusadas.ToString();

            // Mostrar empty state se não houver reservas
            if (_reservas.Count == 0)
            {
                EmptyStateLabel.IsVisible = true;
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Erro de Conexão", $"Não foi possível conectar ao servidor.\n\nVerifique sua conexão com a internet.\n\nDetalhes: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[PROF] Erro de conexão: {ex}");
            EmptyStateLabel.IsVisible = true;
        }
        catch (TaskCanceledException ex)
        {
            await DisplayAlert("Timeout", "A requisição demorou muito para responder.\n\nTente novamente.", "OK");
            System.Diagnostics.Debug.WriteLine($"[PROF] Timeout: {ex}");
            EmptyStateLabel.IsVisible = true;
        }
        catch (JsonException ex)
        {
            await DisplayAlert("Erro de Dados", $"Erro ao processar resposta do servidor.\n\nDetalhes: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[PROF] Erro JSON: {ex}");
            EmptyStateLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao carregar reservas: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[PROF] Exceção LoadReservas: {ex}");
            EmptyStateLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadReservas();
    }

    public ObservableCollection<ProfessorReservaVm> Reservas => _reservas;
}

// ViewModel para exibição
public class ProfessorReservaVm
{
    public string Id { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string QuantityText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Colors.White;
    public string DataText { get; set; } = string.Empty;
}
