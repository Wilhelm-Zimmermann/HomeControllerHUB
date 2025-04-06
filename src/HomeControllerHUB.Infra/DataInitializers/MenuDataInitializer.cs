using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common.Constants;
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
            
            // IoT Menus
            CheckAndCreate("components.menu.iot", "IoT", "dashboard", "iot", "_self", 20);
            
            // Location menu items
            CheckAndCreate("components.menu.iot.locations", "Locais", "map", "iot/locations", "_self", 21, DomainNames.Location, "components.menu.iot");
            
            // Sensor menu items
            CheckAndCreate("components.menu.iot.sensors", "Sensores", "thermometer", "iot/sensors", "_self", 22, DomainNames.Sensor, "components.menu.iot");
            
            // Sensor Data menu items
            CheckAndCreate("components.menu.iot.readings", "Leituras", "activity", "iot/readings", "_self", 23, DomainNames.SensorData, "components.menu.iot");
            CheckAndCreate("components.menu.iot.alerts", "Alertas", "bell", "iot/alerts", "_self", 24, DomainNames.SensorData, "components.menu.iot");
            CheckAndCreate("components.menu.iot.dashboard", "Dashboard", "grid", "iot/dashboard", "_self", 25, DomainNames.SensorData, "components.menu.iot");
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