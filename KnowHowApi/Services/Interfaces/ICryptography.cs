namespace KnowHowApi.Services.Interfaces
{
    public interface ICryptography
    {
        string Crypt(string userPassword);
        bool Verify(string userSubmittedPassword, string storedPassword);
    }
}
