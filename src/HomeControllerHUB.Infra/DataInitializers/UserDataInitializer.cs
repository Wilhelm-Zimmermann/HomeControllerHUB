using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Infra.DataInitializers;

public class UserDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    
    public UserDataInitializer(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : base(5)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public override void InitializeData()
    {
        var establishment = _context.Establishments.FirstOrDefault();
        var profile = _context.Profiles.FirstOrDefault();
        
        CheckAndCreate(establishment!.Id, "Admin", "testes@willow.com.br", "admin", profile!);
    }

    public override void ClearData()
    {
        foreach (var User in _context.Users)
        {
            _context.Users.Remove(User);
        }
        _context.SaveChanges();
    }
    
    private void CheckAndCreate(Guid establishmentId, string name, string mail, string login, Profile profile)
    {
        var exists = _context.Users.AsNoTracking().Any(k => k.Name == name);

        if (!exists)
        {
            var user = new ApplicationUser()
            {
                EstablishmentId = establishmentId,
                Name = name,
                Email = mail,
                Login = login,
                EmailConfirmed = true,
                Enable = true,
                UserName = login,
            };

            var privilege = _context.Privilege.Where(p => p.Name == "platform-all").FirstOrDefault();

            var userProfile = new UserProfile()
            {
                User = user,
                Profile = profile,
            };

            if (privilege != null)
            {
                var profilePrivilege = new ProfilePrivilege()
                {
                    ProfileId = profile.Id,
                    PrivilegeId = privilege.Id,
                };
                _context.ProfilePrivileges.Add(profilePrivilege);
            }

            _context.UserProfiles.Add(userProfile);

            var result = _userManager.CreateAsync(user, "HomeControl#2025").GetAwaiter().GetResult();
            
            Console.WriteLine(result);
        }
    }
}