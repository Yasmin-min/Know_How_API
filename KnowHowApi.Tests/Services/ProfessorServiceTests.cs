using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ProfessorServiceTests
    {
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<IAreaInteresseRepository> _areaInteresseRepositoryMock = new();
        private readonly Mock<IAulaRepository> _aulaRepositoryMock = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<IAulaAgendadaRepository> _aulaAgendadaRepositoryMock = new();
        private readonly Mock<IAvaliacaoRepository> _avaliacaoRepositoryMock = new();
        private readonly ProfessorService _service;

        public ProfessorServiceTests()
        {
            _service = new ProfessorService(_professorRepositoryMock.Object, _areaInteresseRepositoryMock.Object, _aulaRepositoryMock.Object, _usuarioRepositoryMock.Object, _aulaAgendadaRepositoryMock.Object, _avaliacaoRepositoryMock.Object);
        }

        private static PerfilProfessor NovoPerfil(int id = 1, int usuarioId = 10) => new()
        {
            Id = id,
            UsuarioId = usuarioId,
            Materia = "Desenvolvimento Web",
            Avaliacao = 4.9m,
            TotalAvaliacoes = 3,
            Descricao = "Descrição",
            AvatarVariante = "padrao"
        };

        private static CriarAulaRequestDTO AulaAtivaValida() => new()
        {
            Titulo = "Introdução ao React",
            Descricao = "Aprenda React.",
            Materia = "Tecnologia e Programação",
            Valor = 150m,
            DuracaoMinutos = 90,
            Nivel = "Iniciante",
            Status = "ativa"
        };

        private static CriarAulaRequestDTO RascunhoIncompleto() => new()
        {
            Titulo = "",
            Descricao = "",
            Materia = null,
            Valor = null,
            DuracaoMinutos = null,
            Nivel = null,
            Status = "rascunho"
        };

        [Fact]
        public async Task ObterDashboard_ProfessorComPerfil_RetornaEspecialidadeEAvaliacaoReaisComIndicadoresZerados()
        {
            var perfil = NovoPerfil();
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);

            var dashboard = await _service.ObterDashboard(10);

            Assert.NotNull(dashboard);
            Assert.Equal("Desenvolvimento Web", dashboard!.Especialidade);
            Assert.Equal(4.9m, dashboard.Indicadores.AvaliacaoMedia);
            Assert.Equal(0, dashboard.Indicadores.AulasAtivas);
            Assert.Equal(0, dashboard.Indicadores.Alunos);
            Assert.Equal(0, dashboard.Indicadores.AulasSemana);
            Assert.Empty(dashboard.ProximasAulas);
            Assert.Empty(dashboard.AtividadesRecentes);
            Assert.Empty(dashboard.MinhasAulas);
        }

        [Fact]
        public async Task ObterDashboard_ComDados_CalculaIndicadoresEListas()
        {
            var agora = DateTime.UtcNow;
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(NovoPerfil());
            _aulaRepositoryMock.Setup(r => r.ListarDoProfessor(10)).ReturnsAsync(new List<Aula>
            {
                new() { Id = 1, ProfessorId = 10, Titulo = "Rascunho", Descricao = "", Status = "rascunho", CriadaEm = agora },
                new() { Id = 2, ProfessorId = 10, Titulo = "Publicada", Descricao = "", Status = "ativa", CriadaEm = agora.AddDays(-3) }
            });
            _aulaAgendadaRepositoryMock.Setup(r => r.ContarAlunosAtendidos(10, It.IsAny<DateTime>())).ReturnsAsync(4);
            _aulaAgendadaRepositoryMock.Setup(r => r.ListarDoProfessor(10, It.IsAny<DateTime?>(), null)).ReturnsAsync(new List<AulaAgendada>
            {
                new() { Id = Guid.NewGuid(), Titulo = "Já terminou", Inicio = agora.AddHours(-3), Fim = agora.AddHours(-2), Status = AulaAgendada.StatusAgendada },
                new() { Id = Guid.NewGuid(), Titulo = "Daqui a pouco", Aluno = new Usuario { Nome = "Bruno" }, Inicio = agora.AddHours(1), Fim = agora.AddHours(2), Status = AulaAgendada.StatusAgendada },
                new() { Id = Guid.NewGuid(), Titulo = "Cancelada", Inicio = agora.AddHours(3), Fim = agora.AddHours(4), Status = AulaAgendada.StatusCancelada }
            });
            _avaliacaoRepositoryMock.Setup(r => r.ListarPorProfessor(10)).ReturnsAsync(new List<Avaliacao>
            {
                new() { Id = 7, Nota = 5, Comentario = "", Aluno = new Usuario { Nome = "Ana" }, CriadaEm = agora }
            });

            var dashboard = await _service.ObterDashboard(10);

            Assert.Equal(1, dashboard!.Indicadores.AulasAtivas);
            Assert.Equal(4, dashboard.Indicadores.Alunos);
            var proxima = Assert.Single(dashboard.ProximasAulas);
            Assert.Equal("Daqui a pouco", proxima.Titulo);
            Assert.Equal("Bruno", proxima.AlunoNome);
            var atividade = Assert.Single(dashboard.AtividadesRecentes);
            Assert.Equal("avaliacao", atividade.Tipo);
            Assert.Equal("Ana", atividade.NomePessoa);
            Assert.Equal(5, atividade.AvaliacaoEstrelas);
            Assert.Equal(new[] { "Publicada", "Rascunho" }, dashboard.MinhasAulas.Select(a => a.Titulo));
        }

        [Fact]
        public async Task ListarProfessores_IncluiAulasAtivasAgrupadasPorProfessorEVazioQuandoNaoHa()
        {
            var comAulas = NovoPerfil(id: 1, usuarioId: 10);
            comAulas.Usuario = new Usuario { Id = 10, Nome = "Carlos Mendes" };
            var semAulas = NovoPerfil(id: 2, usuarioId: 20);
            semAulas.Usuario = new Usuario { Id = 20, Nome = "Beatriz Rocha" };
            _professorRepositoryMock.Setup(r => r.ListarProfessores()).ReturnsAsync(new List<PerfilProfessor> { comAulas, semAulas });

            _aulaRepositoryMock.Setup(r => r.ListarAtivasDosProfessores(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 10, 20 }))))
                .ReturnsAsync(new List<Aula>
                {
                    new() { Id = 32, ProfessorId = 10, Titulo = "Mais recente", Descricao = "B", Status = "ativa",
                            AreaInteresse = new AreaInteresse { Nome = "Programação" }, Valor = 150m, DuracaoMinutos = 60, Nivel = "Iniciante" },
                    new() { Id = 31, ProfessorId = 10, Titulo = "Mais antiga", Descricao = "A", Status = "ativa" }
                });

            var professores = await _service.ListarProfessores();

            Assert.Equal(new[] { 32, 31 }, professores[0].Aulas.Select(a => a.Id));
            Assert.Equal("Programação", professores[0].Aulas[0].Materia);
            Assert.Null(professores[0].Aulas[1].Materia);
            Assert.Null(professores[0].Aulas[1].Valor);
            Assert.NotNull(professores[1].Aulas);
            Assert.Empty(professores[1].Aulas);
            _aulaRepositoryMock.Verify(r => r.ListarAtivasDosProfessores(It.IsAny<IEnumerable<int>>()), Times.Once);
        }

        [Fact]
        public async Task ObterProfessorPublico_RetornaPerfilComAulasAtivas()
        {
            var perfil = NovoPerfil(id: 1, usuarioId: 10);
            perfil.Usuario = new Usuario { Id = 10, Nome = "Carlos Mendes" };
            _professorRepositoryMock.Setup(r => r.GetPerfilPublico(10)).ReturnsAsync(perfil);
            _aulaRepositoryMock.Setup(r => r.ListarAtivasDosProfessores(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 10 }))))
                .ReturnsAsync(new List<Aula>
                {
                    new() { Id = 32, ProfessorId = 10, Titulo = "Aula publicada", Descricao = "B", Status = "ativa" }
                });

            var professor = await _service.ObterProfessorPublico(10);

            Assert.Equal(10, professor!.Id);
            Assert.Equal("Carlos Mendes", professor.Nome);
            Assert.Equal(new[] { 32 }, professor.Aulas.Select(a => a.Id));
        }

        [Fact]
        public async Task ObterProfessorPublico_Inexistente_RetornaNullSemBuscarAulas()
        {
            _professorRepositoryMock.Setup(r => r.GetPerfilPublico(99)).ReturnsAsync((PerfilProfessor?)null);

            Assert.Null(await _service.ObterProfessorPublico(99));
            _aulaRepositoryMock.Verify(r => r.ListarAtivasDosProfessores(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact]
        public async Task ObterDashboard_ProfessorSemPerfil_RetornaNull()
        {
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(99)).ReturnsAsync((PerfilProfessor?)null);

            var dashboard = await _service.ObterDashboard(99);

            Assert.Null(dashboard);
        }

        [Fact]
        public async Task CriarAula_StatusAtivaComDadosValidos_PersisteEVinculaAoProfessorAutenticado()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>()))
                .ReturnsAsync((Aula a) => { a.Id = 7; return a; });

            var response = await _service.CriarAula(10, AulaAtivaValida());

            Assert.Equal(7, response.Id);
            Assert.Equal("Introdução ao React", response.Titulo);
            Assert.Equal("Tecnologia e Programação", response.Materia);
            Assert.Equal(150m, response.Valor);
            Assert.Equal(90, response.DuracaoMinutos);
            Assert.Equal("Iniciante", response.Nivel);
            Assert.Equal("ativa", response.Status);
            Assert.Equal(0, response.TotalAlunos);

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a =>
                a.ProfessorId == 10 &&
                a.AreaInteresseId == 1 &&
                a.Status == "ativa")), Times.Once);
        }

        [Fact]
        public async Task CriarAula_RascunhoIncompleto_NaoAplicaValidacoesObrigatorias()
        {
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>()))
                .ReturnsAsync((Aula a) => { a.Id = 8; return a; });

            var response = await _service.CriarAula(10, RascunhoIncompleto());

            Assert.Equal("rascunho", response.Status);
            Assert.Equal("", response.Titulo);
            Assert.Equal("", response.Descricao);
            Assert.Null(response.Materia);
            Assert.Null(response.Valor);
            Assert.Null(response.DuracaoMinutos);
            Assert.Null(response.Nivel);
            _areaInteresseRepositoryMock.Verify(r => r.GetAreaInteresseByNome(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemTitulo_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Titulo = "   ";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemDescricao_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Descricao = "";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemMateria_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Materia = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemValor_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Valor = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaComValorZero_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Valor = 0;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_ValorNegativoNoRascunho_LancaDomainValidationException()
        {
            var request = RascunhoIncompleto();
            request.Valor = -1;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_TituloComMaisDeCemCaracteres_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Titulo = new string('a', 101);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_DescricaoComMaisDeQuinhentosCaracteres_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Descricao = new string('a', 501);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_MateriaInexistente_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Área que não existe"))
                .ReturnsAsync((AreaInteresse?)null);
            var request = AulaAtivaValida();
            request.Materia = "Área que não existe";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_NivelInvalido_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            var request = AulaAtivaValida();
            request.Nivel = "Expert";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_DuracaoForaDosValoresPermitidos_LancaDomainValidationException()
        {
            var request = RascunhoIncompleto();
            request.DuracaoMinutos = 75;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_StatusInvalido_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Status = "publicada";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_StatusPausada_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Status = "pausada";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_TituloDuplicadoParaOMesmoProfessor_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(10, "Introdução ao React", null)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, AulaAtivaValida()));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_MesmoTituloParaProfessoresDiferentes_NaoBloqueia()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(10, "Introdução ao React", null)).ReturnsAsync(false);
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>())).ReturnsAsync((Aula a) => a);

            await _service.CriarAula(10, AulaAtivaValida());

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Once);
        }

        [Fact]
        public async Task CriarAula_DoisProfessores_AulaFicaVinculadaSomenteAoProfessorAutenticado()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>())).ReturnsAsync((Aula a) => a);

            await _service.CriarAula(10, AulaAtivaValida());

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a => a.ProfessorId == 10)), Times.Once);
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a => a.ProfessorId == 20)), Times.Never);
        }

        private static Usuario NovoUsuario(int id = 10) => new()
        {
            Id = id,
            Nome = "Professor Teste",
            Email = "professor.teste@knowhow.com",
            Cpf = "71428793860",
            DataNascimento = new DateTime(1988, 11, 2)
        };

        private static PerfilProfessor NovoPerfilComUsuario(Usuario usuario) => new()
        {
            Id = 1,
            UsuarioId = usuario.Id,
            Usuario = usuario,
            Materia = "Tecnologia e Programação",
            AnosExperiencia = "3 a 5 anos",
            Descricao = "Mini biografia.",
            ValorHora = 150m,
            Disponivel = true,
            AvatarVariante = "padrao"
        };

        private static ProfessorPerfilRequestDTO PerfilRequestValido() => new()
        {
            Nome = "Professor Editado",
            Email = "professor.editado@knowhow.com",
            Cpf = "71428793860",
            DataNascimento = new DateTime(1988, 11, 2),
            AreaEspecialidade = "Tecnologia e Programação",
            AnosExperiencia = "5 a 10 anos",
            MiniBiografia = "Nova biografia.",
            ValorHora = 200m,
            Disponivel = false
        };

        [Fact]
        public async Task ObterPerfil_ProfessorExistente_RetornaDadosDoUsuarioEDoPerfil()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);

            var resultado = await _service.ObterPerfil(10);

            Assert.NotNull(resultado);
            Assert.Equal(10, resultado!.Id);
            Assert.Equal("Professor Teste", resultado.Nome);
            Assert.Equal("professor.teste@knowhow.com", resultado.Email);
            Assert.Equal("71428793860", resultado.Cpf);
            Assert.Equal("Tecnologia e Programação", resultado.AreaEspecialidade);
            Assert.Equal("3 a 5 anos", resultado.AnosExperiencia);
            Assert.Equal("Mini biografia.", resultado.MiniBiografia);
            Assert.Equal(150m, resultado.ValorHora);
            Assert.True(resultado.Disponivel);
            Assert.Null(resultado.FotoPerfilBase64);
        }

        [Fact]
        public async Task ObterPerfil_ProfessorSemPerfil_RetornaNull()
        {
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(99)).ReturnsAsync((PerfilProfessor?)null);

            var resultado = await _service.ObterPerfil(99);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarPerfil_DadosValidos_AtualizaUsuarioEPerfilERetornaDadosAtualizados()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(NovoUsuario());
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("professor.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf("71428793860")).ReturnsAsync(usuario);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _usuarioRepositoryMock.Setup(r => r.Update(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);
            _professorRepositoryMock.Setup(r => r.AtualizarPerfilProfessor(It.IsAny<PerfilProfessor>())).ReturnsAsync((PerfilProfessor p) => p);

            var resultado = await _service.AtualizarPerfil(10, PerfilRequestValido());

            Assert.NotNull(resultado);
            Assert.Equal("Professor Editado", resultado!.Nome);
            Assert.Equal("professor.editado@knowhow.com", resultado.Email);
            Assert.Equal("5 a 10 anos", resultado.AnosExperiencia);
            Assert.Equal("Nova biografia.", resultado.MiniBiografia);
            Assert.Equal(200m, resultado.ValorHora);
            Assert.False(resultado.Disponivel);
            _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u => u.Nome == "Professor Editado")), Times.Once);
            _professorRepositoryMock.Verify(r => r.AtualizarPerfilProfessor(It.Is<PerfilProfessor>(p => p.ValorHora == 200m)), Times.Once);
        }

        [Fact]
        public async Task AtualizarPerfil_ProfessorSemPerfil_RetornaNull()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(99)).ReturnsAsync(NovoUsuario(99));
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(99)).ReturnsAsync((PerfilProfessor?)null);

            var resultado = await _service.AtualizarPerfil(99, PerfilRequestValido());

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarPerfil_EmailJaUsadoPorOutroUsuario_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(NovoUsuario());
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("professor.editado@knowhow.com"))
                .ReturnsAsync(new Usuario { Id = 20, Email = "professor.editado@knowhow.com" });

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, PerfilRequestValido()));
            _professorRepositoryMock.Verify(r => r.AtualizarPerfilProfessor(It.IsAny<PerfilProfessor>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_CpfInvalido_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(NovoUsuario());
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);
            var request = PerfilRequestValido();
            request.Cpf = "123";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, request));
            _professorRepositoryMock.Verify(r => r.AtualizarPerfilProfessor(It.IsAny<PerfilProfessor>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_AreaEspecialidadeInexistente_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(NovoUsuario());
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("professor.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf("71428793860")).ReturnsAsync(usuario);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Área que não existe")).ReturnsAsync((AreaInteresse?)null);
            var request = PerfilRequestValido();
            request.AreaEspecialidade = "Área que não existe";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, request));
            _professorRepositoryMock.Verify(r => r.AtualizarPerfilProfessor(It.IsAny<PerfilProfessor>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_ComFotoBase64_AtualizaFotoEContentType()
        {
            var usuario = NovoUsuario();
            var perfil = NovoPerfilComUsuario(NovoUsuario());
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("professor.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf("71428793860")).ReturnsAsync(usuario);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _professorRepositoryMock.Setup(r => r.AtualizarPerfilProfessor(It.IsAny<PerfilProfessor>())).ReturnsAsync((PerfilProfessor p) => p);
            var request = PerfilRequestValido();
            request.FotoPerfilBase64 = "data:image/png;base64,iVBORw0KGgo=";

            var resultado = await _service.AtualizarPerfil(10, request);

            Assert.NotNull(resultado);
            Assert.StartsWith("data:image/png;base64,", resultado!.FotoPerfilBase64);
            _professorRepositoryMock.Verify(r => r.AtualizarPerfilProfessor(It.Is<PerfilProfessor>(p =>
                p.FotoPerfilContentType == "image/png" && p.FotoPerfil != null)), Times.Once);
        }
    }
}
