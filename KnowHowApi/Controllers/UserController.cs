using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowHowApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IUsuarioService _usuarioService;

    public UserController(ILogger<UserController> logger, IUsuarioService usuarioService)
    {
        _logger = logger;
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Route("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    [HttpPost]
    [Route("registrar")]
    [ProducesResponseType(typeof(UsuarioDTO), 200)]
    public async Task<IActionResult> Registrar([FromBody] RegisterUsuarioDTO request)
    {
        TryValidateModel(request);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = await _usuarioService.Registrar(request);
        return Ok(new UsuarioDTO(usuario));
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginResponseDTO), 200)]
    public async Task<IActionResult> ValidateLogin([FromBody] LoginRequestDTO request)
    {
        var login = await _usuarioService.ValidateLogin(request);
        return Ok(login);
    }

    [HttpPost]
    [Route("recuperar-senha/solicitar")]
    [ProducesResponseType(typeof(SolicitarRecuperacaoSenhaResponseDTO), 200)]
    public async Task<IActionResult> SolicitarRecuperacaoSenha([FromBody] SolicitarRecuperacaoSenhaRequestDTO request)
    {
        await _usuarioService.SolicitarRecuperacaoSenha(request.Email);
        return Ok(new SolicitarRecuperacaoSenhaResponseDTO("Se o e-mail informado estiver cadastrado, um código de verificação foi enviado."));
    }

    [HttpPost]
    [Route("recuperar-senha/reenviar")]
    [ProducesResponseType(typeof(SolicitarRecuperacaoSenhaResponseDTO), 200)]
    public async Task<IActionResult> ReenviarRecuperacaoSenha([FromBody] SolicitarRecuperacaoSenhaRequestDTO request)
    {
        await _usuarioService.SolicitarRecuperacaoSenha(request.Email);
        return Ok(new SolicitarRecuperacaoSenhaResponseDTO("Se o e-mail informado estiver cadastrado, um novo código de verificação foi enviado."));
    }

    [HttpPost]
    [Route("recuperar-senha/confirmar")]
    [ProducesResponseType(typeof(ConfirmarCodigoRecuperacaoResponseDTO), 200)]
    public async Task<IActionResult> ConfirmarCodigoRecuperacao([FromBody] ConfirmarCodigoRecuperacaoRequestDTO request)
    {
        var token = await _usuarioService.ConfirmarCodigoRecuperacao(request.Email, request.Codigo);
        return Ok(new ConfirmarCodigoRecuperacaoResponseDTO(token));
    }

    [HttpPost]
    [Route("recuperar-senha/redefinir")]
    [ProducesResponseType(typeof(RedefinirSenhaResponseDTO), 200)]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDTO request)
    {
        await _usuarioService.RedefinirSenha(request.Email, request.TokenRedefinicao, request.NovaSenha);
        return Ok(new RedefinirSenhaResponseDTO("Senha redefinida com sucesso."));
    }

    [Authorize]
    [HttpPut]
    [Route("senha")]
    [ProducesResponseType(typeof(RedefinirSenhaResponseDTO), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequestDTO request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        await _usuarioService.AlterarSenha(usuarioId, request.SenhaAtual, request.NovaSenha);
        return Ok(new RedefinirSenhaResponseDTO("Senha alterada com sucesso."));
    }

    // Foto de perfil como imagem (e não embutida em base64 nas listas). Exige login; o app a busca com o token.
    [Authorize]
    [HttpGet]
    [Route("{usuarioId:int}/foto")]
    [ProducesResponseType(200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ObterFoto([FromRoute] int usuarioId)
    {
        var foto = await _usuarioService.ObterFoto(usuarioId);
        if (foto == null)
            return NoContent();

        var etag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(
            "\"" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(foto.Value.Bytes))[..16] + "\"");
        Response.Headers.CacheControl = "private, max-age=300";
        return File(foto.Value.Bytes, foto.Value.ContentType, lastModified: null, entityTag: etag);
    }
}
