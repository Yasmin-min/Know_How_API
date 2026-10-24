using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AlunoController : Controller
{
    private readonly IAlunoService _alunoService;
    private readonly IAulaAgendadaService _aulaAgendadaService;

    public AlunoController(IAlunoService alunoService, IAulaAgendadaService aulaAgendadaService)
    {
        _alunoService = alunoService;
        _aulaAgendadaService = aulaAgendadaService;
    }

    [Authorize(Roles = "Aluno")]
    [HttpGet]
    [Route("aulas-agendadas")]
    [ProducesResponseType(typeof(List<AulaAgendadaAlunoDTO>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ListarAulasAgendadas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var alunoId))
            return Unauthorized();

        return Ok(await _aulaAgendadaService.ListarAulasDoAluno(alunoId, inicio, fim));
    }

    [Authorize(Roles = "Aluno")]
    [HttpGet]
    [Route("perfil")]
    [ProducesResponseType(typeof(AlunoPerfilResponseDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPerfil()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var perfil = await _alunoService.ObterPerfil(usuarioId);
        if (perfil == null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        return Ok(perfil);
    }

    [Authorize(Roles = "Aluno")]
    [HttpPut]
    [Route("perfil")]
    [ProducesResponseType(typeof(AlunoPerfilResponseDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarPerfil([FromBody] AlunoPerfilRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var perfil = await _alunoService.AtualizarPerfil(usuarioId, request);
        if (perfil == null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        return Ok(perfil);
    }
}
