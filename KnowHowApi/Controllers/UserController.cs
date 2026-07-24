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

    [Authorize]
    [HttpGet]
    [Route("listarTodosOsUsuarios")]
    public async Task<IActionResult> ListarTodosOsUsuarios()
    {
        return Ok(await _usuarioService.ListarTodosOsUsuarios());
    }

    [Authorize]
    [HttpPost]
    [Route("editarUsuario")]
    public async Task<IActionResult> EditarUsuario([FromBody] EditUserDTO editUser)
    {
        TryValidateModel(editUser);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(await _usuarioService.EditarUsuario(editUser.UsuarioId, editUser.UserEditado));
    }
}
