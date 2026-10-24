using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface ICredenciaisTurnService
    {
        // Servidores ICE para o navegador. Sem TURN configurado (ou se a Cloudflare falhar),
        // devolve só STUN públicos: a maioria das conexões funciona, as de redes restritivas não.
        Task<List<IceServerDTO>> GerarIceServers();
    }
}
