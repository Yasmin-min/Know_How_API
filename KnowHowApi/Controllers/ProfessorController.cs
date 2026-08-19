using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfessorController : Controller
{
    private readonly IProfessorService _professorService;

    public ProfessorController(IProfessorService professorService)
    {
        _professorService = professorService;
    }

    [Authorize(Roles = "Aluno")]
    [HttpGet]
    [Route("listar")]
    public async Task<IActionResult> Listar()
    {
        return Ok(await _professorService.ListarProfessores());
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("dashboard")]
    [ProducesResponseType(typeof(ProfessorDashboardResponseDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Dashboard()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var dashboard = await _professorService.ObterDashboard(usuarioId);
        if (dashboard == null)
            return NotFound(new { mensagem = "Perfil de professor não encontrado para o usuário autenticado." });

        return Ok(dashboard);
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("aulas")]
    [ProducesResponseType(typeof(ProfessorAulaResponseDTO), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarAula([FromBody] CriarAulaRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        var aula = await _professorService.CriarAula(professorId, request);
        return StatusCode(StatusCodes.Status201Created, aula);
    }
}
