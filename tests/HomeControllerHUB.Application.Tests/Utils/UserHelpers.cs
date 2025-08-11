using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Utils;

public class UserHelpers
{
    public static ApplicationUser GetApplicationUser()
    {
        return new ApplicationUser()
        {
            Name = "teste",
            Email = "teste@gmai.com",
            Login = "testekkkkk",
            EmailConfirmed = true,
            Enable = true,
            UserName = "lsldkfj",
            Code = "UserShared",
            NormalizedName = "TESTE",
            PasswordHash = "thisisahashdonotignore"
        };
    }
}