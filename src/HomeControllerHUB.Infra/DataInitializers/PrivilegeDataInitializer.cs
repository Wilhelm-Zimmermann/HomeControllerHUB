using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common.Constants;

namespace HomeControllerHUB.Infra.DataInitializers;

public class PrivilegeDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;

    public PrivilegeDataInitializer(ApplicationDbContext context) : base(3)
    {
        _context = context;
    }

    public override void InitializeData()
    {
        CheckAndCreate(DomainNames.All, PrivilegeNames.All, "Todas as permissões", SecurityActionType.All);
        CheckAndCreate(DomainNames.Establishment, PrivilegeNames.EstalishmentAll, "Estabelecimentos - Tudo", SecurityActionType.All);
        CheckAndCreate(DomainNames.Establishment, PrivilegeNames.EstalishmentRead, "Estabelecimentos - Ver", SecurityActionType.Read);
        CheckAndCreate(DomainNames.Establishment, PrivilegeNames.EstalishmentCreate, "Estabelecimentos - Criar", SecurityActionType.Create);
        CheckAndCreate(DomainNames.Establishment, PrivilegeNames.EstalishmentUpdate, "Estabelecimentos - Alterar", SecurityActionType.Update);
        CheckAndCreate(DomainNames.Establishment, PrivilegeNames.EstalishmentDelete, "Estabelecimentos - Excluir", SecurityActionType.Delete);
        CheckAndCreate(DomainNames.User, PrivilegeNames.UserAll, "Usuários - Tudo", SecurityActionType.All);
        CheckAndCreate(DomainNames.User, PrivilegeNames.UserRead, "Usuários - Ver", SecurityActionType.Read);
        CheckAndCreate(DomainNames.User, PrivilegeNames.UserCreate, "Usuários - Criar", SecurityActionType.Create);
        CheckAndCreate(DomainNames.User, PrivilegeNames.UserUpdate, "Usuários - Alterar", SecurityActionType.Update);
        CheckAndCreate(DomainNames.User, PrivilegeNames.UserDelete, "Usuários - Excluir", SecurityActionType.Delete);
        CheckAndCreate(DomainNames.Profile, PrivilegeNames.ProfileAll, "Perfis de usuário - Tudo", SecurityActionType.All);
        CheckAndCreate(DomainNames.Profile, PrivilegeNames.ProfileRead, "Perfis de usuário - Ver", SecurityActionType.Read);
        CheckAndCreate(DomainNames.Profile, PrivilegeNames.ProfileCreate, "Perfis de usuário - Criar", SecurityActionType.Create);
        CheckAndCreate(DomainNames.Profile, PrivilegeNames.ProfileUpdate, "Perfis de usuário - Alterar", SecurityActionType.Update);
        CheckAndCreate(DomainNames.Profile, PrivilegeNames.ProfileDelete, "Perfis de usuário - Excluir", SecurityActionType.Delete);
        CheckAndCreate(DomainNames.Privilege, PrivilegeNames.PrivilegeRead, "Privilégio - Ler", SecurityActionType.Read);
    }
    
    private void CheckAndCreate(string domainName, string name, string description, string action)
    {
        var exists = _context.Privilege.AsQueryable().Any(k => k.Name == name);
        var estalishment = _context.Establishments.FirstOrDefault();

        if (!exists)
        {
            var domain = _context.Domains.Where(p => p.Name == domainName).FirstOrDefault();

            var entity = new Privilege()
            {
                EstablishmentId = estalishment.Id,
                DomainId = domain!.Id,
                Name = name,
                Description = description,
                Actions = action,
                Enable = true
            };

            _context.Privilege.Add(entity);
            _context.SaveChanges();
        }
    }

    public override void ClearData()
    {
        foreach (var Priv in _context.Privilege)
        {
            _context.Privilege.Remove(Priv);
        }
        _context.SaveChanges();
    }
}