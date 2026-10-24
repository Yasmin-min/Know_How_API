using KnowHowApi.Domain.Exceptions;

namespace KnowHowApi.Services.Utils
{
    public static class FotoPerfilConverter
    {
        public static (byte[] Bytes, string? ContentType) ParseBase64(string fotoPerfilBase64)
        {
            string? contentType = null;
            var base64 = fotoPerfilBase64;

            if (fotoPerfilBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var separatorIndex = fotoPerfilBase64.IndexOf(',');
                if (separatorIndex < 0)
                    throw new DomainValidationException("Foto de perfil inválida.");

                var header = fotoPerfilBase64[5..separatorIndex];
                var contentTypeEnd = header.IndexOf(';');
                contentType = contentTypeEnd >= 0 ? header[..contentTypeEnd] : header;
                base64 = fotoPerfilBase64[(separatorIndex + 1)..];
            }

            try
            {
                return (Convert.FromBase64String(base64), contentType);
            }
            catch (FormatException)
            {
                throw new DomainValidationException("Foto de perfil inválida.");
            }
        }

        public static string? ToDataUri(byte[]? bytes, string? contentType)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            return $"data:{contentType ?? "image/jpeg"};base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
