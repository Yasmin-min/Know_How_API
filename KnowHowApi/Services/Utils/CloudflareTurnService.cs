using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace KnowHowApi.Services.Utils
{
    public class TurnSettings
    {
        // Cloudflare Realtime > TURN: "Turn Token ID" e "API Token". Vazios = só STUN.
        public string CloudflareKeyId { get; set; } = string.Empty;
        public string CloudflareApiToken { get; set; } = string.Empty;
        // Validade das credenciais geradas para cada entrada na sala.
        public int TtlSegundos { get; set; } = 4 * 60 * 60;
    }

    // Gera credenciais TURN temporárias na Cloudflare. O token da Cloudflare fica só no servidor;
    // o navegador recebe credenciais que expiram sozinhas.
    public class CloudflareTurnService : ICredenciaisTurnService
    {
        private static readonly List<IceServerDTO> SomenteStun = new()
        {
            new IceServerDTO { Urls = new() { "stun:stun.cloudflare.com:3478", "stun:stun.l.google.com:19302" } }
        };

        // Navegadores bloqueiam a porta 53; a Cloudflare recomenda remover essas URLs no cliente.
        private static readonly Regex Porta53 = new(@":53(\?|$)", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly TurnSettings _settings;
        private readonly ILogger<CloudflareTurnService> _logger;

        public CloudflareTurnService(HttpClient httpClient, IOptions<TurnSettings> settings, ILogger<CloudflareTurnService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<List<IceServerDTO>> GerarIceServers()
        {
            if (string.IsNullOrWhiteSpace(_settings.CloudflareKeyId) || string.IsNullOrWhiteSpace(_settings.CloudflareApiToken))
                return SomenteStun;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://rtc.live.cloudflare.com/v1/turn/keys/{_settings.CloudflareKeyId}/credentials/generate-ice-servers")
                {
                    Content = JsonContent.Create(new { ttl = _settings.TtlSegundos })
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.CloudflareApiToken);

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var servidores = LerIceServers(json.RootElement);
                return servidores.Count > 0 ? servidores : SomenteStun;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao gerar credenciais TURN na Cloudflare; usando só STUN.");
                return SomenteStun;
            }
        }

        // A resposta traz "iceServers" como lista (ou como um único objeto, em versões antigas da API).
        public static List<IceServerDTO> LerIceServers(JsonElement raiz)
        {
            if (!raiz.TryGetProperty("iceServers", out var iceServers))
                return new List<IceServerDTO>();

            var itens = iceServers.ValueKind == JsonValueKind.Array
                ? iceServers.EnumerateArray().ToList()
                : new List<JsonElement> { iceServers };

            var resultado = new List<IceServerDTO>();
            foreach (var item in itens)
            {
                if (!item.TryGetProperty("urls", out var urlsJson))
                    continue;

                var urls = (urlsJson.ValueKind == JsonValueKind.Array
                        ? urlsJson.EnumerateArray().Select(u => u.GetString())
                        : new[] { urlsJson.GetString() })
                    .Where(u => !string.IsNullOrEmpty(u) && !Porta53.IsMatch(u))
                    .Select(u => u!)
                    .ToList();
                if (urls.Count == 0)
                    continue;

                resultado.Add(new IceServerDTO
                {
                    Urls = urls,
                    Username = item.TryGetProperty("username", out var usuario) ? usuario.GetString() : null,
                    Credential = item.TryGetProperty("credential", out var credencial) ? credencial.GetString() : null
                });
            }
            return resultado;
        }
    }
}
