using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ReservaChro.Mobile.Views;

public partial class LoginPage : ContentPage
{
    // Android Emulator -> localhost do PC
    private const string ApiBaseUrl = "http://10.0.2.2:5193";

    // SchoolIds (banco)
    private static readonly Guid SchoolIdDiadema = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SchoolIdSbc = Guid.Parse("cea0f35d-7b03-44c2-b365-bc59cda6c073");

    // ✅ NOVO COLÉGIO (exemplo)
    //private static readonly Guid SchoolIdNovo = Guid.Parse("COLE-AQUI-O-UUID-DO-NOVO-COLEGIO");

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var username = EmailEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;

            // Diagnóstico: confirma se o app está capturando os campos corretamente
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert(
                    "Atenção",
                    $"Campos vazios detectados.\n" +
                    $"Username: {(string.IsNullOrWhiteSpace(username) ? "(vazio)" : username)}\n" +
                    $"Password: {(string.IsNullOrWhiteSpace(password) ? "(vazio)" : "***")}",
                    "OK"
                );
                return;
            }

            var http = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };

            // Contrato REAL do seu AuthController: Username + Password
            var response = await http.PostAsJsonAsync("/auth/login", new
            {
                username,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Login inválido",
                    $"Status: {(int)response.StatusCode}\n{body}",
                    "OK");
                return;
            }

            // Lê a resposta do backend (LoginResponseDto) para pegar SchoolId com segurança
            var responseJson = await response.Content.ReadAsStringAsync();

            // Token (para chamadas futuras)
            var token = TryExtractToken(responseJson);

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert("Erro", "Login OK, mas não encontrei o token na resposta.", "OK");
                return;
            }

            await SecureStorage.SetAsync("auth_token", token);

            // Name/Role via JWT (ok para UI)
            var (name, role) = TryReadNameAndRoleFromJwt(token, username);

            // Regra: Somente Professor pode entrar nas telas de colégio
            if (!string.Equals(role, "Professor", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Acesso negado", "Esta tela é exclusiva para Professor.", "OK");
                return;
            }

            // SchoolId via resposta do backend (fonte de verdade para roteamento)
            var schoolId = TryExtractSchoolId(responseJson);
            if (schoolId is null)
            {
                await DisplayAlert("Erro", "Login OK, mas não encontrei SchoolId na resposta.", "OK");
                return;
            }

            // Roteamento por escola
            if (schoolId.Value == SchoolIdDiadema)
            {
                await Navigation.PushAsync(new ColegioDiademaPage(name, role));
                return;
            }

            if (schoolId.Value == SchoolIdSbc)
            {
                await Navigation.PushAsync(new ColegioSbcPage(name, role));
                return;
            }

            // ✅ NOVO COLÉGIO (exemplo)
            //if (schoolId.Value == SchoolIdNovo)
            //{
            // await Navigation.PushAsync(new ColegioNovoPage(name, role));
            // return;
            //}

            await DisplayAlert("Sem rota",
                $"Professor autenticado, mas a escola não está mapeada no app.\nSchoolId: {schoolId}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private static Guid? TryExtractSchoolId(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // LoginResponseDto tem SchoolId
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("schoolId", out var sid) && sid.ValueKind == JsonValueKind.String)
                {
                    if (Guid.TryParse(sid.GetString(), out var g)) return g;
                }

                // Caso venha "SchoolId" com maiúscula
                if (root.TryGetProperty("SchoolId", out var sid2))
                {
                    if (sid2.ValueKind == JsonValueKind.String && Guid.TryParse(sid2.GetString(), out var g2)) return g2;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractToken(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("token", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString();
                if (root.TryGetProperty("accessToken", out var at) && at.ValueKind == JsonValueKind.String) return at.GetString();
                if (root.TryGetProperty("jwt", out var j) && j.ValueKind == JsonValueKind.String) return j.GetString();

                // seu backend retorna LoginResponseDto com Token (provável)
                if (root.TryGetProperty("Token", out var tk) && tk.ValueKind == JsonValueKind.String) return tk.GetString();
                if (root.TryGetProperty("token", out var tk2) && tk2.ValueKind == JsonValueKind.String) return tk2.GetString();

                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    if (data.TryGetProperty("token", out var dt) && dt.ValueKind == JsonValueKind.String) return dt.GetString();
                    if (data.TryGetProperty("accessToken", out var dat) && dat.ValueKind == JsonValueKind.String) return dat.GetString();
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static (string name, string role) TryReadNameAndRoleFromJwt(string token, string fallbackUsername)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return (fallbackUsername, "Usuário");

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            string name =
                TryGetString(root, "name") ??
                TryGetString(root, "unique_name") ??
                TryGetString(root, "given_name") ??
                fallbackUsername;

            string roleRaw =
                TryGetString(root, "role") ??
                TryGetString(root, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ??
                "Usuário";

            var role = roleRaw switch
            {
                "1" => "Admin",
                "2" => "TI",
                "3" => "Professor",
                _ => roleRaw
            };

            return (name, role);
        }
        catch
        {
            return (fallbackUsername, "Usuário");
        }
    }

    private static string? TryGetString(JsonElement root, string prop)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        if (!root.TryGetProperty(prop, out var v)) return null;
        return v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
    }

    private static byte[] Base64UrlDecode(string base64Url)
    {
        var s = base64Url.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
