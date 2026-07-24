using Microsoft.AspNetCore.SignalR;

namespace KnowHowApi.Hubs
{
    // Placeholder do chat em tempo real entre professor e aluno.
    // As entradas/grupos por conversa (ex: por aula/negociação) serão definidas
    // quando o domínio de Chat/Conversa for implementado.
    public class ChatHub : Hub
    {
        public async Task EnviarMensagem(string conversaId, string remetente, string mensagem)
        {
            await Clients.Group(conversaId).SendAsync("ReceberMensagem", remetente, mensagem);
        }

        public async Task EntrarNaConversa(string conversaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversaId);
        }
    }
}
