namespace HomeControllerHUB.Domain.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string Login { get; }
    Guid EstablishmentId { get; }
}