using System.Text.Json;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace KnowHowApi.Tests.Services.Utils
{
    public class ExceptionHandlingMiddlewareTests
    {
        private static async Task<(int StatusCode, string? Message)> ExecutarAsync(RequestDelegate next)
        {
            var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();

            string? message = null;
            if (!string.IsNullOrEmpty(body))
            {
                using var json = JsonDocument.Parse(body);
                message = json.RootElement.GetProperty("message").GetString();
            }

            return (context.Response.StatusCode, message);
        }

        [Fact]
        public async Task InvokeAsync_DomainValidationException_Retorna400ComMensagemEmJson()
        {
            var (statusCode, message) = await ExecutarAsync(_ => throw new DomainValidationException("Área de interesse inválida."));

            Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
            Assert.Equal("Área de interesse inválida.", message);
        }

        [Fact]
        public async Task InvokeAsync_BadHttpRequestException_Retorna400ComMensagemEmJson()
        {
            var (statusCode, message) = await ExecutarAsync(_ => throw new BadHttpRequestException("Informe e-mail e senha"));

            Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
            Assert.Equal("Informe e-mail e senha", message);
        }

        [Fact]
        public async Task InvokeAsync_ExcecaoNaoMapeada_Retorna500ComMensagemGenericaSemStackTrace()
        {
            var (statusCode, message) = await ExecutarAsync(_ => throw new InvalidOperationException("detalhe interno sensível, linha X do arquivo Y"));

            Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
            Assert.Equal("Ocorreu um erro interno. Tente novamente mais tarde.", message);
            Assert.DoesNotContain("detalhe interno sensível", message);
        }

        [Fact]
        public async Task InvokeAsync_SemExcecao_NaoAlteraResposta()
        {
            var (statusCode, message) = await ExecutarAsync(_ => Task.CompletedTask);

            Assert.Equal(StatusCodes.Status200OK, statusCode);
            Assert.Null(message);
        }
    }
}
