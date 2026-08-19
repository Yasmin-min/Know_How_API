using KnowHowApi.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace KnowHowApi.Services.Utils
{
    // Garante que toda exceção vire uma resposta JSON { "message": "..." } e nunca
    // vaze como texto puro/stack trace (DeveloperExceptionPageMiddleware) para o frontend.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainValidationException ex)
            {
                await EscreverErroAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                var statusCode = ex.StatusCode >= 400 && ex.StatusCode < 600 ? ex.StatusCode : StatusCodes.Status400BadRequest;
                await EscreverErroAsync(context, statusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno não tratado.");
                await EscreverErroAsync(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.");
            }
        }

        private static async Task EscreverErroAsync(HttpContext context, int statusCode, string mensagem)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = mensagem });
        }
    }
}
