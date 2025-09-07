namespace LevelUpClone.Application.Abstractions
{
    public interface IUserService
    {
        Task<bool> ValidateAsync(string userName, string password);
        Task SetPasswordAsync(string userName, string newPassword);
    }
}
