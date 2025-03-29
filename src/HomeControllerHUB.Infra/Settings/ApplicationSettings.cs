namespace HomeControllerHUB.Infra.Settings;

public class ApplicationSettings
{
    public JwtSettings JwtSettings { get; set; }
    public HostSettings HostSettings { get; set; }
    public SwaggerSettings SwaggerSettings { get; set; }
    public string? InitializeDataBase { get; set; }
}