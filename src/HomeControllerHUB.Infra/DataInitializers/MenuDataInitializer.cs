using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Utils;

namespace HomeControllerHUB.Infra.DataInitializers;

public class MenuDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;

    public MenuDataInitializer(ApplicationDbContext context) : base(7)
    {
        _context = context;
    }
    
    public override void InitializeData()
        {
            CheckAndCreate("components.menu.home", "Início", "house", "/", "_self", 1);
            // Cadastros
            // CheckAndCreate("components.menu.register.registers", "Cadastros", "register", "cadastros", "_self", 11);
            // CheckAndCreate("components.menu.register.currency", "Moedas", "currency", "cadastros/moedas", "_self", 11, DomainNames.Currency, "components.menu.register.registers");
        }
    
    public override void ClearData()
    {
        foreach (var item in _context.Menus)
        {
            _context.Menus.Remove(item);
        }
        _context.SaveChanges();
    }


    private void CheckAndCreate(string name, string description, string IconClass, string Link, string Target, int Order, string domainName = "", string parentName = "")
    {
        var exists = _context.Menus.AsQueryable().Any(k => k.Name == name);

        if (!exists)
        {
            ApplicationDomain domain = null;
            if (domainName.HasValue())
            {
                domain = _context.Domains.Where(p => p.Name == domainName).FirstOrDefault();
            }

            ApplicationMenu parent = null;
            if (parentName.HasValue())
            {
                parent = _context.Menus.Where(p => p.Name == parentName).FirstOrDefault();
            }

            var entity = new ApplicationMenu()
            {
                DomainId = domain is null ? null : domain.Id,
                ParentId = parent is null ? null : parent.Id,
                Name = name,
                NormalizedName = name.ToUpper(),
                Description = description,
                NormalizedDescription = description.ToUpper(),
                IconClass = IconClass,
                Link = Link,
                Target = Target,
                Order = Order,
                Enable = true
            };

            _context.Menus.Add(entity);
            _context.SaveChanges();
        }
    }
}