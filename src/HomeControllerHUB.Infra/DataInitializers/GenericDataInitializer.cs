using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;

namespace HomeControllerHUB.Infra.DataInitializers;

public class GenericDataInitializer : BaseDataInitializer, IDataInitializer
{
    private readonly ApplicationDbContext _context;

    public GenericDataInitializer(ApplicationDbContext context) : base(6)
    {
        _context = context;
    }
    
    public override void InitializeData()
    {
        
    }

    public override void ClearData()
    {
        foreach (var generic in _context.Generics)
        {
            _context.Generics.Remove(generic);
        }
        _context.SaveChanges();
    }
    
    private void CheckAndCreate(string identifier, string name, string code, int displayOrder)
    {
        var exists = _context.Generics.AsQueryable().Any(k => k.Value == name);

        if (!exists)
        {
            var entity = new Generic()
            {
                Identifier = identifier,
                Value = name,
                NormalizedValue = name.ToUpper(),
                Code = code,
                DisplayOrder = displayOrder,
                Enable = true
            };

            _context.Generics.Add(entity);
            _context.SaveChanges();
        }
    }
}