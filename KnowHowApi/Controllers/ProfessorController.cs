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
}
