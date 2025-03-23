using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;

namespace HomeControllerHUB.Infra.DataInitializers;

public class EstablishmentDataInitializer :  BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;
    
    public EstablishmentDataInitializer(ApplicationDbContext context) : base(1)
    {
        _context = context;
    }
    
    public override void InitializeData()
    {
        CheckAndCreate(
            "noDoc", 
            "WillHome", 
            "WillHome",
            true,
            true
        );
    }

    public override void ClearData()
    {
        foreach (var Establishment in _context.Establishments)
        {
            _context.Establishments.Remove(Establishment);
        }
        _context.SaveChanges();
    }
    
    private void CheckAndCreate(
        string document, 
        string name, 
        string siteName,
        bool enable,
        bool isMaster
    )
    {
        var exists = _context.Establishments.AsQueryable().Any(k => k.Name == name);

        if (!exists)
        {
            var entity = new Establishment()
            {
                Document = document,
                Name = name,
                SiteName = siteName,
                IsMaster = isMaster,
                Enable = enable
            };

            _context.Establishments.Add(entity);
            _context.SaveChanges();
        }
    }
}