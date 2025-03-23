namespace HomeControllerHUB.Infra.DataInitializers;

public interface IDataInitializer
{
    void InitializeData();

    void ClearData();

    int Order { get; set; }
}