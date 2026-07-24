using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services.Utils
{
    public class Cryptography : ICryptography
    {
        public string Crypt(string userPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(userPassword);
        }

        public bool Verify(string userSubmittedPassword, string storedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(userSubmittedPassword, storedPassword);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
