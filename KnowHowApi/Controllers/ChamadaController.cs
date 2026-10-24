using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

// Sala de vídeo das aulas agendadas. A sinalização WebRTC em si passa pelo ChatHub.
[ApiController]
[Route("[controller]")]
public class ChamadaController : Controller
{
    private readonly IChamadaService _chamadaService;

    public ChamadaController(IChamadaService chamadaService)
    {
        _chamadaService = chamadaService;
    }

    [Authorize(Roles = "Aluno,Professor")]
    [HttpGet]
    [Route("{aulaId:guid}")]
    [ProducesResponseType(typeof(ChamadaAcessoDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterAcesso([FromRoute] Guid aulaId)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        return Ok(await _chamadaService.ObterAcesso(usuarioId, aulaId));
    }
}
