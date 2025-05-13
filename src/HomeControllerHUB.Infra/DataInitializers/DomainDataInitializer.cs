using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common.Constants;

namespace HomeControllerHUB.Infra.DataInitializers;

public class DomainDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;
    
    public DomainDataInitializer(ApplicationDbContext context) : base(2)
    {
        _context = context;
    }

    public override void InitializeData()
    {
        CheckAndCreate(DomainNames.All, "Todos");
        CheckAndCreate(DomainNames.Establishment, "Estabelecimentos");
        CheckAndCreate(DomainNames.User, "Usuários do Sistema");
        CheckAndCreate(DomainNames.Profile, "Perfis de Usuário");
        CheckAndCreate(DomainNames.Privilege, "Privilégios");
        
        // IoT Domains
        CheckAndCreate(DomainNames.Location, "Locais");
        CheckAndCreate(DomainNames.Sensor, "Sensores");
        CheckAndCreate(DomainNames.SensorData, "Dados de Sensores");
        CheckAndCreate(DomainNames.IoT, "Iot Logica");
    }

    public override void ClearData()
    {
        foreach (var Domain in _context.Domains)
        {
            _context.Domains.Remove(Domain);
        }
        _context.SaveChanges();
    }
    
    private void CheckAndCreate(string name, string description)
    {
        var exists = _context.Domains.AsQueryable().Any(k => k.Name == name);

        if (!exists)
        {
            var entity = new ApplicationDomain()
            {
                Name = name,
                NormalizedName = name.ToUpper(),
                Description = description,
                NormalizedDescription = description.ToUpper(),
                Enable = true
            };

            _context.Domains.Add(entity);
            _context.SaveChanges();
        }
    }
}