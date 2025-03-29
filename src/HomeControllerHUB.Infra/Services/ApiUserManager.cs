using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Infra.Services;

public class ApiUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationDbContext _context;

    public ApiUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, ApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(
        store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services,
        logger)
    {
        _context = context;
    }

    public virtual new async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
    {
        ThrowIfDisposed();

        var userRoles = await GetRolesAsync(user);
        return userRoles.Contains(role);
    }

    public new virtual async Task<IList<Privilege>> GetPrivilegesAsync(ApplicationUser user)
    {
        var roles = await GetRolesAsync(user);
        var privileges = new List<Privilege>();

        return privileges.DistinctBy(p => p.Id).ToList();
    }

    public virtual async Task<IdentityResult> SetAuthenticationTokenAsync(ApplicationUser user, string loginProvider,
        string tokenName, string? tokenValue)
    {
        ThrowIfDisposed();
        var store = Store as IUserAuthenticationTokenStore<ApplicationUser>;
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (loginProvider == null)
        {
            throw new ArgumentNullException(nameof(loginProvider));
        }

        if (tokenName == null)
        {
            throw new ArgumentNullException(nameof(tokenName));
        }

        await store.SetTokenAsync(user, loginProvider, tokenName, tokenValue, CancellationToken).ConfigureAwait(false);
        await UpdateNormalizedUserNameAsync(user).ConfigureAwait(false);
        await UpdateNormalizedEmailAsync(user).ConfigureAwait(false);
        return await Store.UpdateAsync(user, CancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<ApplicationUser?> FindByEmailAsync(string email, Guid? EstablishmentId = null)
    {
        var validUsers = await GetUsersFromEstablishment(null, email, EstablishmentId);

        return validUsers.FirstOrDefault(u => u.Email == email);
    }

    private async Task<List<ApplicationUser>> GetUsersFromEstablishment(string userName = null, string email = null,
        Guid? EstablishmentId = null)
    {
        var establishments = _context.Establishments.AsQueryable();
        establishments = establishments.Where(e => e.Id == EstablishmentId);

        var users = establishments.SelectMany(e => e.Users);

        users = users.Where(u => u.Email == email);

        return users.ToList();
    }
}