namespace KnowHowApi.Domain.Exceptions
{
    // Recurso inexistente (ou de outro usuário) que deve virar 404 com { "message": "..." }.
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
