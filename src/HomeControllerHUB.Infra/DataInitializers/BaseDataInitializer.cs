namespace HomeControllerHUB.Infra.DataInitializers;

public abstract class BaseDataInitializer : IDataInitializer
{
    public BaseDataInitializer(int order) : base()
    {
        Order = order;
    }

    public int Order { get; set; }

    public abstract void InitializeData();

    public abstract void ClearData();
}