namespace LevelUpClone.Infrastructure.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
        bool NeedsUpgrade(string hash);
    }
}
