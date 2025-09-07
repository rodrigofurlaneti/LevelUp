using System;
namespace LevelUpClone.Application.Abstractions
{
    public interface IUserService
    {
        Task<bool> ValidateAsync(string UserName, string Password);
        Task SetPasswordAsync(int UserId, string NewPassword);
    }
}
