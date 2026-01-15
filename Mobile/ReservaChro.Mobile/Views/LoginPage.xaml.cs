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

            // Contrato REAL do seu AuthController:
            // request.Username + request.Password
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

            var json = await response.Content.ReadAsStringAsync();
            var token = TryExtractToken(json);

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert("Erro", "Login OK, mas não encontrei o token na resposta.", "OK");
                return;
            }

            await SecureStorage.SetAsync("auth_token", token);

            var (name, role) = TryReadNameAndRoleFromJwt(token, username);

            await Navigation.PushAsync(new ColegioDiademaPage(name, role));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
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

                // seu backend retorna LoginResponseDto com Token
                if (root.TryGetProperty("Token", out var tk) && tk.ValueKind == JsonValueKind.String) return tk.GetString();

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
