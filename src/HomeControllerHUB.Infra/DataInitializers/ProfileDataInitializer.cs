using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;

namespace HomeControllerHUB.Infra.DataInitializers;

public class ProfileDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;
    
    public ProfileDataInitializer(ApplicationDbContext context) : base(2)
    {
        _context = context;
    }

    public override void InitializeData()
    {
        CheckAndCreate("Admin root", "Todos os acessos");
        CheckAndCreate("Cliente", "Cliente do sistema");
    }

    public override void ClearData()
    {
        foreach (var Profile in _context.Profiles)
        {
            _context.Profiles.Remove(Profile);
        }
        _context.SaveChanges();
    }
    
    private void CheckAndCreate(string name, string description)
    {
        var exists = _context.Profiles.AsQueryable().Any(k => k.Name == name);

        if (!exists)
        {
            var establishment = _context.Establishments.FirstOrDefault();

            var entity = new Profile()
            {
                EstablishmentId = establishment!.Id,
                Name = name,
                Description = description,
                Enable = true
            };

            _context.Profiles.Add(entity);
            _context.SaveChanges();
        }
    }
}