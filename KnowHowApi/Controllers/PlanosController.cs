using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

// Preços, pacotes, planos mensais e contratações do professor. Fica sob /Professor
// (mesmo prefixo do ProfessorController) para seguir o contrato do frontend.
[ApiController]
[Route("Professor")]
public class PlanosController : Controller
{
    private readonly IPlanosService _planosService;

    public PlanosController(IPlanosService planosService)
    {
        _planosService = planosService;
    }

    private bool TryGetUsuarioId(out int usuarioId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId);
    }

    // Preços

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("precos")]
    [ProducesResponseType(typeof(ConfiguracaoPrecosDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPrecos()
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.ObterPrecos(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("precos")]
    [ProducesResponseType(typeof(ConfiguracaoPrecosDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarPrecos([FromBody] AtualizarPrecosRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.AtualizarPrecos(professorId, request));
    }

    // Pacotes

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("pacotes")]
    [ProducesResponseType(typeof(List<PacoteDTO>), 200)]
    public async Task<IActionResult> ListarPacotes()
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.ListarPacotes(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("pacotes")]
    [ProducesResponseType(typeof(PacoteDTO), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarPacote([FromBody] SalvarPacoteRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        var pacote = await _planosService.CriarPacote(professorId, request);
        return StatusCode(StatusCodes.Status201Created, pacote);
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("pacotes/{id:guid}")]
    [ProducesResponseType(typeof(PacoteDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarPacote([FromRoute] Guid id, [FromBody] SalvarPacoteRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.AtualizarPacote(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpDelete]
    [Route("pacotes/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirPacote([FromRoute] Guid id)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        await _planosService.ExcluirPacote(professorId, id);
        return NoContent();
    }

    // Planos mensais

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("planos-mensais")]
    [ProducesResponseType(typeof(List<PlanoMensalDTO>), 200)]
    public async Task<IActionResult> ListarPlanosMensais()
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.ListarPlanosMensais(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("planos-mensais")]
    [ProducesResponseType(typeof(PlanoMensalDTO), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarPlanoMensal([FromBody] SalvarPlanoMensalRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        var plano = await _planosService.CriarPlanoMensal(professorId, request);
        return StatusCode(StatusCodes.Status201Created, plano);
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("planos-mensais/{id:guid}")]
    [ProducesResponseType(typeof(PlanoMensalDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarPlanoMensal([FromRoute] Guid id, [FromBody] SalvarPlanoMensalRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.AtualizarPlanoMensal(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpDelete]
    [Route("planos-mensais/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirPlanoMensal([FromRoute] Guid id)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        await _planosService.ExcluirPlanoMensal(professorId, id);
        return NoContent();
    }

    // Ofertas (vistas pelo aluno no perfil do professor)

    [Authorize(Roles = "Aluno,Professor")]
    [HttpGet]
    [Route("{professorId:int}/ofertas")]
    [ProducesResponseType(typeof(OfertasProfessorDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterOfertas([FromRoute] int professorId)
    {
        return Ok(await _planosService.ObterOfertas(professorId));
    }

    // Contratações

    [Authorize(Roles = "Professor")]
    [HttpGet]
    [Route("contratacoes")]
    [ProducesResponseType(typeof(List<ContratacaoDTO>), 200)]
    public async Task<IActionResult> ListarContratacoes()
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.ListarContratacoes(professorId));
    }

    [Authorize(Roles = "Professor")]
    [HttpPost]
    [Route("contratacoes")]
    [ProducesResponseType(typeof(ContratacaoDTO), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarContratacao([FromBody] CriarContratacaoRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        var contratacao = await _planosService.CriarContratacao(professorId, request);
        return StatusCode(StatusCodes.Status201Created, contratacao);
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("contratacoes/{id:guid}/pagamento")]
    [ProducesResponseType(typeof(ContratacaoDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarPagamento([FromRoute] Guid id, [FromBody] AlterarPagamentoContratacaoRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.AlterarPagamento(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpPut]
    [Route("contratacoes/{id:guid}/aulas-usadas")]
    [ProducesResponseType(typeof(ContratacaoDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarAulasUsadas([FromRoute] Guid id, [FromBody] AlterarAulasUsadasRequestDTO request)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        return Ok(await _planosService.AlterarAulasUsadas(professorId, id, request));
    }

    [Authorize(Roles = "Professor")]
    [HttpDelete]
    [Route("contratacoes/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirContratacao([FromRoute] Guid id)
    {
        if (!TryGetUsuarioId(out var professorId))
            return Unauthorized();

        await _planosService.ExcluirContratacao(professorId, id);
        return NoContent();
    }
}
