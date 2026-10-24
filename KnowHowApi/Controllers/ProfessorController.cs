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
    private readonly IAulaAgendadaService _aulaAgendadaService;
    private readonly IProfessorAlunoService _professorAlunoService;

    public ProfessorController(IProfessorService professorService, IAulaAgendadaService aulaAgendadaService, IProfessorAlunoService professorAlunoService)
    {
        _professorService = professorService;
        _aulaAgendadaService = aulaAgendadaService;
        _professorAlunoService = professorAlunoService;
    }

    [Authorize(Roles = "Aluno,Professor")]
    [HttpGet]
    [Route("listar")]
    public async Task<IActionResult> Listar()
    {
        return Ok(await _professorService.ListarProfessores());
    }

    [Authorize(Roles = "Aluno,Professor")]
    [HttpGet]
    [Route("{id:int}/publico")]
    [ProducesResponseType(typeof(ProfessorDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPublico([FromRoute] int id)
    {
        var professor = await _professorService.ObterProfessorPublico(id);
        if (professor == null)
            return NotFound(new { mensagem = "Professor não encontrado." });

        return Ok(professor);
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

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("aulas")]
    [ProducesResponseType(typeof(List<ProfessorAulaResponseDTO>), 200)]
    public async Task<IActionResult> ListarAulas()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorService.ListarAulas(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("aulas/{id:int}")]
    [ProducesResponseType(typeof(ProfessorAulaResponseDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterAula([FromRoute] int id)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorService.ObterAula(professorId, id));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("aulas/{id:int}")]
    [ProducesResponseType(typeof(ProfessorAulaResponseDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarAula([FromRoute] int id, [FromBody] CriarAulaRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorService.AtualizarAula(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("aulas/{id:int}/status")]
    [ProducesResponseType(typeof(ProfessorAulaResponseDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarStatusAula([FromRoute] int id, [FromBody] AlterarStatusAulaRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorService.AlterarStatusAula(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("perfil")]
    [ProducesResponseType(typeof(ProfessorPerfilResponseDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPerfil()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var perfil = await _professorService.ObterPerfil(usuarioId);
        if (perfil == null)
            return NotFound(new { mensagem = "Perfil de professor não encontrado para o usuário autenticado." });

        return Ok(perfil);
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("perfil")]
    [ProducesResponseType(typeof(ProfessorPerfilResponseDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarPerfil([FromBody] ProfessorPerfilRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var perfil = await _professorService.AtualizarPerfil(usuarioId, request);
        if (perfil == null)
            return NotFound(new { mensagem = "Perfil de professor não encontrado para o usuário autenticado." });

        return Ok(perfil);
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("alunos")]
    [ProducesResponseType(typeof(List<AlunoResumoDTO>), 200)]
    public async Task<IActionResult> ListarAlunos()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorAlunoService.ListarAlunos(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("alunos")]
    [ProducesResponseType(typeof(AlunoResumoDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AdicionarAluno([FromBody] AdicionarAlunoRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorAlunoService.AdicionarAlunoPorEmail(professorId, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("alunos/{alunoId:int}/notas")]
    [ProducesResponseType(typeof(NotaAlunoDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterNotaAluno([FromRoute] int alunoId)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorAlunoService.ObterNota(professorId, alunoId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("alunos/{alunoId:int}/notas")]
    [ProducesResponseType(typeof(NotaAlunoDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SalvarNotaAluno([FromRoute] int alunoId, [FromBody] SalvarNotaAlunoRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _professorAlunoService.SalvarNota(professorId, alunoId, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("aulas-agendadas")]
    [ProducesResponseType(typeof(List<AulaAgendadaDTO>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ListarAulasAgendadas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _aulaAgendadaService.ListarAulas(professorId, inicio, fim));
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("aulas-agendadas")]
    [ProducesResponseType(typeof(AulaAgendadaDTO), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CriarAulaAgendada([FromBody] CriarAulaAgendadaRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        var aula = await _aulaAgendadaService.CriarAula(professorId, request);
        return StatusCode(StatusCodes.Status201Created, aula);
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("aulas-agendadas/{id:guid}")]
    [ProducesResponseType(typeof(AulaAgendadaDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> EditarAulaAgendada([FromRoute] Guid id, [FromBody] EditarAulaAgendadaRequestDTO request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _aulaAgendadaService.EditarAula(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("aulas-agendadas/{id:guid}/cancelar")]
    [ProducesResponseType(typeof(AulaAgendadaDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelarAulaAgendada([FromRoute] Guid id)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var professorId))
            return Unauthorized();

        return Ok(await _aulaAgendadaService.CancelarAula(professorId, id));
    }
}
