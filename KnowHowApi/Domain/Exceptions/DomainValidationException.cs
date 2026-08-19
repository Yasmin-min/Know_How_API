namespace KnowHowApi.Domain.Exceptions
{
    // Erro de validação/regra de negócio que deve virar 400 com { "message": "..." },
    // nunca vazar como exceção não tratada (stack trace/DeveloperExceptionPage).
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string message) : base(message)
        {
        }
    }
}
