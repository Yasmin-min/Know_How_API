using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    [Route("conversas")]
    [ProducesResponseType(typeof(List<ConversaResumoDTO>), 200)]
    public async Task<IActionResult> ListarConversas()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        return Ok(await _chatService.ListarConversas(usuarioId.Value));
    }

    [HttpGet]
    [Route("conversas/{id}/mensagens")]
    [ProducesResponseType(typeof(List<MensagemDTO>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListarMensagens(string id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        if (!int.TryParse(id, out var conversaId))
            return NotFound(new { mensagem = "Conversa não encontrada." });

        var mensagens = await _chatService.ListarMensagens(conversaId, usuarioId.Value);
        if (mensagens == null)
            return NotFound(new { mensagem = "Conversa não encontrada." });

        return Ok(mensagens);
    }

    [HttpPost]
    [Route("conversas/{id}/mensagens")]
    [ProducesResponseType(typeof(MensagemDTO), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> EnviarMensagem(string id, [FromBody] EnviarMensagemRequestDTO request)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        if (!int.TryParse(id, out var conversaId))
            return NotFound(new { mensagem = "Conversa não encontrada." });

        var mensagem = await _chatService.EnviarMensagem(conversaId, usuarioId.Value, request);
        if (mensagem == null)
            return NotFound(new { mensagem = "Conversa não encontrada." });

        return StatusCode(StatusCodes.Status201Created, mensagem);
    }

    [Authorize(Roles = "Aluno")]
    [HttpPost]
    [Route("conversas")]
    [ProducesResponseType(typeof(ConversaResumoDTO), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> IniciarConversa([FromBody] IniciarConversaRequestDTO request)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        var conversa = await _chatService.IniciarConversa(usuarioId.Value, request);
        return Ok(conversa);
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("conversas/aluno")]
    [ProducesResponseType(typeof(ConversaResumoDTO), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> IniciarConversaComAluno([FromBody] IniciarConversaComAlunoRequestDTO request)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        var conversa = await _chatService.IniciarConversaComAluno(usuarioId.Value, request);
        return Ok(conversa);
    }

    [HttpPut]
    [Route("conversas/{id}/marcar-lida")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarComoLida(string id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null)
            return Unauthorized();

        if (!int.TryParse(id, out var conversaId))
            return NotFound(new { mensagem = "Conversa não encontrada." });

        var resultado = await _chatService.MarcarComoLida(conversaId, usuarioId.Value);
        if (resultado == null)
            return NotFound(new { mensagem = "Conversa não encontrada." });

        return Ok(new { mensagem = "Mensagens marcadas como lidas." });
    }

    private int? ObterUsuarioId()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(usuarioIdClaim, out var usuarioId) ? usuarioId : null;
    }
}
