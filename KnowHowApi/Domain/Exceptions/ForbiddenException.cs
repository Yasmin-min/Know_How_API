namespace KnowHowApi.Domain.Exceptions
{
    // Usuário autenticado sem permissão para a ação; vira 403 com { "message": "..." }.
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
