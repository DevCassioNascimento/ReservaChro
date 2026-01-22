using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ReservaChro.Mobile.Views;

public partial class LoginPage : ContentPage
{
    // Android Emulator -> localhost do PC
    private const string ApiBaseUrl = "http://10.0.2.2:5193";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert(
                    "Atenção",
                    $"Campos vazios detectados.\n" +
                    $"Email: {(string.IsNullOrWhiteSpace(email) ? "(vazio)" : email)}\n" +
                    $"Password: {(string.IsNullOrWhiteSpace(password) ? "(vazio)" : "***")}",
                    "OK"
                );
                return;
            }

            using var http = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };

            // ✅ Envia email (novo padrão)
            // ✅ Envia username também (compatibilidade com backend antigo)
            var response = await http.PostAsJsonAsync("/auth/login", new
            {
                email,             // novo
                username = email,  // compatibilidade
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                await DisplayAlert(
                    "Login inválido",
                    $"Status: {(int)response.StatusCode}\n{body}",
                    "OK"
                );
                return;
            }

            // Lê a resposta do backend (LoginResponseDto) para pegar Token + SchoolId
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
            var (name, role) = TryReadNameAndRoleFromJwt(token, email);

            // SchoolId via resposta do backend (fonte de verdade)
            var schoolId = TryExtractSchoolId(responseJson);
            if (schoolId is null)
            {
                await DisplayAlert("Erro", "Login OK, mas não encontrei SchoolId na resposta.", "OK");
                return;
            }

            // ✅ Roteamento por perfil
            // TI (aceita "TI" ou "2")
            if (string.Equals(role, "TI", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "2", StringComparison.OrdinalIgnoreCase))
            {
                await Navigation.PushAsync(new TiDashboardPage(name, role, schoolId.Value, token));
                return;
            }

            // Professor (aceita "Professor" ou "3")
            if (string.Equals(role, "Professor", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "3", StringComparison.OrdinalIgnoreCase))
            {
                await Navigation.PushAsync(
                    new ProfessorSchoolPage(
                        schoolId: schoolId.Value,
                        userName: name,
                        roleName: "Professor"
                    )
                );
                return;
            }

            // Outros perfis: negar
            await DisplayAlert("Acesso negado", "Perfil não autorizado para esta área.", "OK");
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

            if (root.ValueKind == JsonValueKind.Object)
            {
                // schoolId (camel)
                if (root.TryGetProperty("schoolId", out var sid) && sid.ValueKind == JsonValueKind.String)
                    return Guid.TryParse(sid.GetString(), out var g) ? g : null;

                // SchoolId (Pascal)
                if (root.TryGetProperty("SchoolId", out var sid2) && sid2.ValueKind == JsonValueKind.String)
                    return Guid.TryParse(sid2.GetString(), out var g2) ? g2 : null;

                // caso venha aninhado em "data"
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    if (data.TryGetProperty("schoolId", out var dsid) && dsid.ValueKind == JsonValueKind.String)
                        return Guid.TryParse(dsid.GetString(), out var dg) ? dg : null;

                    if (data.TryGetProperty("SchoolId", out var dsid2) && dsid2.ValueKind == JsonValueKind.String)
                        return Guid.TryParse(dsid2.GetString(), out var dg2) ? dg2 : null;
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

                // LoginResponseDto pode vir PascalCase
                if (root.TryGetProperty("Token", out var tk) && tk.ValueKind == JsonValueKind.String) return tk.GetString();

                // caso venha aninhado em "data"
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    if (data.TryGetProperty("token", out var dt) && dt.ValueKind == JsonValueKind.String) return dt.GetString();
                    if (data.TryGetProperty("accessToken", out var dat) && dat.ValueKind == JsonValueKind.String) return dat.GetString();
                    if (data.TryGetProperty("Token", out var dtk) && dtk.ValueKind == JsonValueKind.String) return dtk.GetString();
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static (string name, string role) TryReadNameAndRoleFromJwt(string token, string fallbackEmail)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return (fallbackEmail, "Usuário");

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            string name =
                TryGetString(root, "name") ??
                TryGetString(root, "unique_name") ??
                TryGetString(root, "given_name") ??
                fallbackEmail;

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
            return (fallbackEmail, "Usuário");
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
