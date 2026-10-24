namespace KnowHowApi.Domain.Exceptions
{
    // Conflito com o estado atual (ex.: horário já ocupado) que deve virar 409 com { "message": "..." }.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
