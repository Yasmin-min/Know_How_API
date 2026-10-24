using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

// Avaliações de alunos sobre professores. As rotas seguem o contrato do frontend:
// leitura sob /Professor e escrita sob /Aluno.
[ApiController]
public class AvaliacaoController : Controller
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacaoController(IAvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    private bool TryGetUsuarioId(out int usuarioId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId);
    }

    [Authorize(Roles = "Aluno,Professor")]
    [HttpGet]
    [Route("Professor/{professorId:int}/avaliacoes")]
    [ProducesResponseType(typeof(AvaliacoesProfessorDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListarDoProfessor([FromRoute] int professorId)
    {
        if (!TryGetUsuarioId(out var usuarioId))
            return Unauthorized();

        int? alunoLogadoId = User.IsInRole("Aluno") ? usuarioId : null;
        return Ok(await _avaliacaoService.ListarDoProfessor(professorId, alunoLogadoId));
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("Professor/avaliacoes")]
    [ProducesResponseType(typeof(AvaliacoesProfessorDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListarRecebidas()
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _avaliacaoService.ListarDoProfessor(professorId, null));
    }

    [Authorize(Roles = "Aluno")]
    [HttpPut]
    [Route("Aluno/avaliacoes/{professorId:int}")]
    [ProducesResponseType(typeof(AvaliacaoDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Salvar([FromRoute] int professorId, [FromBody] SalvarAvaliacaoRequestDTO request)
    {
        if (!TryGetUsuarioId(out var alunoId))
            return Unauthorized();

        return Ok(await _avaliacaoService.Salvar(alunoId, professorId, request));
    }

    [Authorize(Roles = "Aluno")]
    [HttpDelete]
    [Route("Aluno/avaliacoes/{professorId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir([FromRoute] int professorId)
    {
        if (!TryGetUsuarioId(out var alunoId))
            return Unauthorized();

        await _avaliacaoService.Excluir(alunoId, professorId);
        return NoContent();
    }
}
