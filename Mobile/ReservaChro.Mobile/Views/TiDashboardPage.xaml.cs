// ReservaChro\Mobile\ReservaChro.Mobile\Views\TiDashboardPage.xaml.cs
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReservaChro.Mobile.Views;

public partial class TiDashboardPage : ContentPage
{
    private readonly Guid _schoolId;
    private string _token;
    private string _apiBaseUrl;

    private readonly string _name;
    private readonly string _role;

    // SchoolIds (banco) - usados só para mapear nome na UI
    private static readonly Guid SchoolIdDiadema = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Diadema
    private static readonly Guid SchoolIdSbc = Guid.Parse("cea0f35d-7b03-44c2-b365-bc59cda6c073"); // São Bernardo do Campo
    private static readonly Guid SchoolIdEaa = Guid.Parse("b10550e8-a08e-4647-a44d-c7635a44c240"); // Americanopolis
    private static readonly Guid SchoolIdCacia = Guid.Parse("0f1e3953-40ca-4a8b-95ad-e2f06fdd3d83"); // Cidade Ademar
    private static readonly Guid SchoolIdCadg = Guid.Parse("5699b934-93e3-4419-91ff-513f3d645d06"); // Guaruja
    private static readonly Guid SchoolIdCai = Guid.Parse("9a529bf0-e491-48f5-8453-e569e668f856"); // Interlagos
    private static readonly Guid SchoolIdCaju = Guid.Parse("57764f1f-0a29-4969-a5a8-5a7106fef8c1"); // Jardim Utinga
    private static readonly Guid SchoolIdCam = Guid.Parse("4956125d-d1e6-49bf-a15a-870c3c43dfcb"); // Maua
    private static readonly Guid SchoolIdCap = Guid.Parse("b33dd6fb-e1d0-4f51-ac99-4b5a20d8f8cf"); // Pedreira
    private static readonly Guid SchoolIdCapg = Guid.Parse("102308b1-5588-4704-a83d-b25dd810fbeb"); // Praia Grande
    private static readonly Guid SchoolIdCarr = Guid.Parse("2fd8108d-3bba-4234-b993-7ef2aeae9b99"); // Rudge Ramos
    private static readonly Guid SchoolIdCasa = Guid.Parse("00b01fa8-b32f-45d7-9434-ad519e921992"); // Santo André
    private static readonly Guid SchoolIdCas = Guid.Parse("e8c26a70-1eb6-419d-a9b5-3b6a16abcbc0"); // Santos
    private static readonly Guid SchoolIdCascs = Guid.Parse("985c5eb2-4268-4ed6-bcd9-e2726e36fd91"); // São Caetano do Sul

    public ObservableCollection<TiBookingVm> Bookings { get; } = new();
    public ObservableCollection<TiAgendaVm> AgendaReservas { get; } = new();

    public TiDashboardPage(string name, string role, Guid schoolId, string token = "")
    {
        InitializeComponent();
        BindingContext = this;

        _name = string.IsNullOrWhiteSpace(name) ? "Usuário" : name.Trim();
        _role = string.IsNullOrWhiteSpace(role) ? "TI" : role.Trim();
        _schoolId = schoolId;

        _token = token ?? string.Empty;

        // ✅ Base URL robusta por plataforma
        _apiBaseUrl = ResolveApiBaseUrl();

        ApplyHeader();
        ApplySchoolName();

        SeedMock();

        // Carrega API quando a página aparecer (melhor do que no construtor)
        this.Appearing += async (_, __) =>
        {
            await EnsureTokenAsync();
            await LoadEstoqueData();
            await LoadReservasPendentes();
        };

        SetTab("reservas");
    }

    // Compatibilidade caso o XAML Previewer use construtor vazio
    public TiDashboardPage() : this("Usuário", "TI", Guid.Empty, "") { }

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
        if (!string.IsNullOrWhiteSpace(_token))
            return;

        try
        {
            var stored = await SecureStorage.GetAsync("auth_token");
            _token = stored ?? string.Empty;
        }
        catch
        {
            _token = string.Empty;
        }
    }

    private void ApplyHeader()
    {
        UserNameLabel.Text = _name;

        if (_role.Equals("TI", StringComparison.OrdinalIgnoreCase) ||
            _role.Equals("Profissional de TI", StringComparison.OrdinalIgnoreCase))
            UserRoleLabel.Text = "Profissional de TI";
        else
            UserRoleLabel.Text = _role;
    }

    private void ApplySchoolName()
    {
        SchoolNameLabel.Text = MapSchoolName(_schoolId);
    }

    private static string MapSchoolName(Guid schoolId)
    {
        if (schoolId == Guid.Empty)
            return "Escola não identificada";

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

        // Stats padrão (será atualizado pela API)
        PendentesValue.Text = "2";
        EmUsoValue.Text = "1";
        ConfirmadasValue.Text = "1";
        DisponivelValue.Text = "0";
        EstoqueTotalValue.Text = "0";
    }

    // ===== API Integration =====
    private async Task LoadReservasPendentes()
    {
        try
        {
            await EnsureTokenAsync();

            if (string.IsNullOrWhiteSpace(_token))
            {
                return;
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.GetAsync($"{_apiBaseUrl}/reservas/pendentes");
            var json = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] GET /reservas/pendentes -> {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[TI] Response Body: {json}");

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[TI] Erro ao buscar reservas: {json}");
                // Se falhar, mantém dados mock mas limpa a lista
                Bookings.Clear();
                PendentesValue.Text = "0";
                return;
            }

            // Deserializar resposta com opções que aceitam PascalCase
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Aceita tanto camelCase quanto PascalCase
            };

            List<ReservaResponseDto> reservas;
            
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    // Resposta direta como array
                    reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(json, options)
                        ?? new List<ReservaResponseDto>();
                }
                else if (doc.RootElement.TryGetProperty("reservas", out var reservasElement))
                {
                    // Resposta com envelope { schoolId, reservas, total }
                    reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(reservasElement.GetRawText(), options)
                        ?? new List<ReservaResponseDto>();
                }
                else
                {
                    reservas = new List<ReservaResponseDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TI] Erro ao deserializar: {ex.Message}");
                // Fallback: tentar deserializar direto
                reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(json, options)
                    ?? new List<ReservaResponseDto>();
            }

            // Log detalhado para debug
            if (reservas.Count > 0)
            {
                var primeira = reservas[0];
                System.Diagnostics.Debug.WriteLine($"[TI] Primeira reserva - Id: {primeira.Id}, ProfessorNome: {primeira.ProfessorNome}, DataReserva: {primeira.DataReserva}, Quantidade: {primeira.Quantidade}, Status: {primeira.Status}");
            }

            System.Diagnostics.Debug.WriteLine($"[TI] Reservas encontradas: {reservas.Count}");

            Bookings.Clear();

            foreach (var reserva in reservas)
            {
                var statusText = reserva.Status switch
                {
                    1 => "Pendente",
                    2 => "Confirmada",
                    3 => "Recusada",
                    4 => "Em uso",
                    5 => "Concluída",
                    _ => "Desconhecido"
                };

                var statusColor = reserva.Status switch
                {
                    1 => Color.FromArgb("#7a5b16"), // Pendente - laranja
                    2 => Color.FromArgb("#1c4f86"),  // Confirmada - azul
                    4 => Color.FromArgb("#1f6b4a"),  // Em uso - verde
                    _ => Colors.Gray
                };

                // Validar se os dados estão corretos antes de adicionar
                if (reserva.Id == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[TI] Reserva com ID vazio ignorada");
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

                Bookings.Add(new TiBookingVm
                {
                    Id = reserva.Id.ToString(),
                    ProfessorName = string.IsNullOrWhiteSpace(reserva.ProfessorNome) ? "Professor" : reserva.ProfessorNome,
                    DateText = dataFormatada,
                    Time = horarioFormatado,
                    QuantityText = quantidadeTexto,
                    StatusText = statusText,
                    StatusColor = statusColor,
                    ShowConfirm = reserva.Status == 1, // Pendente
                    ShowRecusar = reserva.Status == 1, // Pendente
                    ShowIniciarUso = reserva.Status == 2, // Confirmada
                    ShowDevolucao = reserva.Status == 4 // Em uso
                });
            }

            // Atualizar contador de pendentes
            PendentesValue.Text = reservas.Count(r => r.Status == 1).ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção LoadReservasPendentes: {ex}");
        }
    }

    private async Task LoadEstoqueData()
    {
        try
        {
            await EnsureTokenAsync();

            if (string.IsNullOrWhiteSpace(_token))
            {
                EstoqueTotalValue.Text = "Sem token";
                DisponivelValue.Text = "0";
                return;
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.GetAsync($"{_apiBaseUrl}/school/estoque");
            var json = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] GET /school/estoque -> {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[TI] Body: {json}");

            if (!response.IsSuccessStatusCode)
            {
                EstoqueTotalValue.Text = $"Erro {(int)response.StatusCode}";
                DisponivelValue.Text = "0";
                return;
            }

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("quantidade", out var quantidadeElement))
            {
                var quantidade = quantidadeElement.GetInt32();
                EstoqueTotalValue.Text = quantidade.ToString();

                // por enquanto "Disponível" = total (depois você separa por status real)
                DisponivelValue.Text = quantidade.ToString();
                return;
            }

            EstoqueTotalValue.Text = "Erro JSON";
            DisponivelValue.Text = "0";
        }
        catch (Exception ex)
        {
            EstoqueTotalValue.Text = "Erro ao carregar";
            DisponivelValue.Text = "0";
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção LoadEstoqueData: {ex}");
        }
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

        // Recarregar reservas quando a aba de reservas for selecionada
        if (tab == "reservas")
        {
            _ = LoadReservasPendentes();
        }
        // Recarregar agenda quando a aba de agenda for selecionada
        else if (tab == "agenda")
        {
            _ = LoadAgendaReservas();
        }
    }

    private static void SetTabButton(Button btn, bool active)
    {
        btn.BackgroundColor = active ? Color.FromArgb("#1b63ff") : Colors.Transparent;
        btn.TextColor = active ? Colors.White : Color.FromArgb("#a8c4ff");
    }

    // ===== Actions =====
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            SecureStorage.Remove("auth_token");
        }
        catch { /* ignore */ }

        await Navigation.PopToRootAsync();
    }

    private async void OnConfirmarClicked(object sender, EventArgs e)
    {
        var idString = (sender as Button)?.CommandParameter?.ToString();
        if (string.IsNullOrWhiteSpace(idString) || !Guid.TryParse(idString, out var reservaId))
        {
            await DisplayAlert("Erro", "ID da reserva inválido.", "OK");
            return;
        }

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

            var response = await client.PutAsync($"{_apiBaseUrl}/reservas/{reservaId}/confirmar", null);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] PUT /reservas/{reservaId}/confirmar -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "✅ Reserva confirmada com sucesso!", "OK");
                // Recarregar lista de reservas
                await LoadReservasPendentes();
                await LoadEstoqueData();
            }
            else
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao confirmar reserva.\n\n{mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao confirmar reserva: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnConfirmarClicked: {ex}");
        }
    }

    private async void OnRecusarClicked(object sender, EventArgs e)
    {
        var idString = (sender as Button)?.CommandParameter?.ToString();
        if (string.IsNullOrWhiteSpace(idString) || !Guid.TryParse(idString, out var reservaId))
        {
            await DisplayAlert("Erro", "ID da reserva inválido.", "OK");
            return;
        }

        // Confirmar ação
        var confirmar = await DisplayAlert(
            "Confirmar Recusa",
            "Tem certeza que deseja recusar esta reserva?",
            "Sim, Recusar",
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

            var response = await client.PutAsync($"{_apiBaseUrl}/reservas/{reservaId}/recusar", null);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] PUT /reservas/{reservaId}/recusar -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "✅ Reserva recusada com sucesso!", "OK");
                // Recarregar lista de reservas (o card será removido automaticamente pois só mostra pendentes)
                await LoadReservasPendentes();
            }
            else
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao recusar reserva.\n\n{mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao recusar reserva: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnRecusarClicked: {ex}");
        }
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

    // Atualiza a quantidade total de estoque da escola (simplificado)
    private async void OnAtualizarEstoqueClicked(object sender, EventArgs e)
    {
        try
        {
            await EnsureTokenAsync();

            var novoEstoque = NovoEstoqueEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(novoEstoque) || !int.TryParse(novoEstoque, out int quantidadeDesejada))
            {
                await DisplayAlert("Erro", "Digite um número válido de máquinas", "OK");
                return;
            }

            if (quantidadeDesejada < 0)
            {
                await DisplayAlert("Erro", "Digite um número maior ou igual a 0", "OK");
                return;
            }

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

            // Atualizar estoque diretamente
            var request = new { quantidade = quantidadeDesejada };
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/school/estoque", request);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] PUT /school/estoque -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            // Recarregar dados do estoque
            await LoadEstoqueData();

            NovoEstoqueEntry.Text = "";

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", $"✅ Estoque atualizado para {quantidadeDesejada} máquina(s)!\nEstoque atual: {EstoqueTotalValue.Text}", "OK");
            }
            else
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() 
                    : $"Erro {(int)response.StatusCode}";
                
                await DisplayAlert("Erro", $"❌ Falha ao atualizar estoque.\n\n{mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao atualizar estoque: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnAtualizarEstoqueClicked: {ex}");
        }
    }

    private async void OnAlterarSenhaClicked(object sender, EventArgs e)
    {
        await EnsureTokenAsync();

        if (string.IsNullOrWhiteSpace(_token))
        {
            await DisplayAlert("Erro", "Token não encontrado. Faça login novamente.", "OK");
            return;
        }

        // Solicitar senha atual
        var senhaAtual = await DisplayPromptAsync(
            "Alterar Senha",
            "Digite sua senha atual:",
            "OK",
            "Cancelar",
            "",
            -1,
            Keyboard.Default,
            "");

        if (string.IsNullOrWhiteSpace(senhaAtual))
            return;

        // Solicitar nova senha
        var novaSenha = await DisplayPromptAsync(
            "Alterar Senha",
            "Digite a nova senha (mínimo 4 caracteres):",
            "OK",
            "Cancelar",
            "",
            -1,
            Keyboard.Default,
            "");

        if (string.IsNullOrWhiteSpace(novaSenha))
            return;

        if (novaSenha.Length < 4)
        {
            await DisplayAlert("Erro", "A nova senha deve ter pelo menos 4 caracteres.", "OK");
            return;
        }

        // Confirmar nova senha
        var confirmarSenha = await DisplayPromptAsync(
            "Alterar Senha",
            "Confirme a nova senha:",
            "OK",
            "Cancelar",
            "",
            -1,
            Keyboard.Default,
            "");

        if (string.IsNullOrWhiteSpace(confirmarSenha))
            return;

        if (novaSenha != confirmarSenha)
        {
            await DisplayAlert("Erro", "As senhas não coincidem.", "OK");
            return;
        }

        // Enviar requisição para alterar senha
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var request = new
            {
                currentPassword = senhaAtual,
                newPassword = novaSenha
            };

            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/auth/change-password", request);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] PUT /auth/change-password -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "✅ Senha alterada com sucesso!", "OK");
            }
            else
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(body);
                    var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                        ? msgElement.GetString()
                        : $"Erro {(int)response.StatusCode}";

                    await DisplayAlert("Erro", $"❌ Falha ao alterar senha.\n\n{mensagemErro}", "OK");
                }
                catch
                {
                    await DisplayAlert("Erro", $"❌ Falha ao alterar senha.\n\nErro {(int)response.StatusCode}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao alterar senha: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnAlterarSenhaClicked: {ex}");
        }
    }

    private async Task LoadAgendaReservas()
    {
        try
        {
            AgendaLoadingIndicator.IsRunning = true;
            AgendaLoadingIndicator.IsVisible = true;
            AgendaEmptyLabel.IsVisible = false;

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

            var response = await client.GetAsync($"{_apiBaseUrl}/reservas/agenda");
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] GET /reservas/agenda -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (!response.IsSuccessStatusCode)
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao carregar agenda.\n\n{mensagemErro}", "OK");
                return;
            }

            // Deserializar resposta
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<ReservaResponseDto>? reservas = null;

            try
            {
                reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(body, options);
            }
            catch
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(dataElement.GetRawText(), options);
                    }
                    else if (doc.RootElement.TryGetProperty("reservas", out var reservasElement))
                    {
                        reservas = System.Text.Json.JsonSerializer.Deserialize<List<ReservaResponseDto>>(reservasElement.GetRawText(), options);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TI] Erro ao deserializar agenda: {ex}");
                }
            }

            if (reservas == null)
            {
                reservas = new List<ReservaResponseDto>();
            }

            System.Diagnostics.Debug.WriteLine($"[TI] Total de reservas na agenda: {reservas.Count}");

            // Limpar e atualizar lista
            AgendaReservas.Clear();

            foreach (var reserva in reservas)
            {
                // Determinar status text e color
                string statusText;
                Color statusColor;
                string dataText = reserva.DataCriacao.ToString("dd/MM/yyyy HH:mm");

                switch (reserva.Status)
                {
                    case 2: // Confirmada
                        statusText = "✅ Confirmada";
                        statusColor = Color.FromArgb("#6bcf7f");
                        break;
                    case 4: // EmUso
                        statusText = "🔄 Em Uso";
                        statusColor = Color.FromArgb("#4a9eff");
                        break;
                    default:
                        statusText = "❓ Desconhecido";
                        statusColor = Colors.Gray;
                        break;
                }

                // Validar se os dados estão corretos antes de adicionar
                if (reserva.Id == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[TI] Reserva com ID vazio ignorada na agenda");
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

                AgendaReservas.Add(new TiAgendaVm
                {
                    Id = reserva.Id.ToString(),
                    ProfessorName = string.IsNullOrWhiteSpace(reserva.ProfessorNome) ? "Professor" : reserva.ProfessorNome,
                    DateText = dataFormatada,
                    Time = horarioFormatado,
                    QuantityText = quantidadeTexto,
                    StatusText = statusText,
                    StatusColor = statusColor,
                    DataText = dataText,
                    ShowDevolver = reserva.Status == 4 // Apenas para Em Uso
                });
            }

            // Mostrar empty state se não houver reservas
            if (AgendaReservas.Count == 0)
            {
                AgendaEmptyLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao carregar agenda: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção LoadAgendaReservas: {ex}");
        }
        finally
        {
            AgendaLoadingIndicator.IsRunning = false;
            AgendaLoadingIndicator.IsVisible = false;
        }
    }

    private async void OnDevolverClicked(object sender, EventArgs e)
    {
        var idString = (sender as Button)?.CommandParameter?.ToString();
        if (string.IsNullOrWhiteSpace(idString) || !Guid.TryParse(idString, out var reservaId))
        {
            await DisplayAlert("Erro", "ID da reserva inválido.", "OK");
            return;
        }

        // Confirmar ação
        var confirmar = await DisplayAlert(
            "Confirmar Devolução",
            "Tem certeza que deseja marcar esta reserva como devolvida?",
            "Sim, Devolver",
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

            var response = await client.PutAsync($"{_apiBaseUrl}/reservas/{reservaId}/devolver", null);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[TI] PUT /reservas/{reservaId}/devolver -> {(int)response.StatusCode} {response.StatusCode} | {body}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "✅ Reserva concluída com sucesso!", "OK");
                // Recarregar agenda (o card será removido automaticamente)
                await LoadAgendaReservas();
            }
            else
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                var mensagemErro = doc.RootElement.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : $"Erro {(int)response.StatusCode}";

                await DisplayAlert("Erro", $"❌ Falha ao devolver reserva.\n\n{mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao devolver reserva: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[TI] Exceção OnDevolverClicked: {ex}");
        }
    }
}

public class ChromestoqueResponseDto
{
    public Guid Id { get; set; }
    public string NomeMaquina { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataAquisicao { get; set; }
    public Guid SchoolId { get; set; }
}

public class ReservaResponseDto
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("professorId")]
    public Guid ProfessorId { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("professorNome")]
    public string ProfessorNome { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("schoolId")]
    public Guid SchoolId { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("dataReserva")]
    public DateTime DataReserva { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("horarioInicio")]
    public TimeSpan HorarioInicio { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("horarioFim")]
    public TimeSpan HorarioFim { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("quantidade")]
    public int Quantidade { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public int Status { get; set; } // 1=Pendente, 2=Confirmada, 3=Recusada, 4=EmUso, 5=Concluida
    
    [System.Text.Json.Serialization.JsonPropertyName("dataCriacao")]
    public DateTime DataCriacao { get; set; }
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

// ViewModel para Agenda
public class TiAgendaVm
{
    public string Id { get; set; } = string.Empty;
    public string ProfessorName { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string QuantityText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Colors.White;
    public string DataText { get; set; } = string.Empty;
    public bool ShowDevolver { get; set; } = false;
}
