namespace tulo.CoreLib.Interfaces.Services
{
    public interface IAccountService
    {
        string GetUsername(Guid id);
        string GetEmail(Guid id);
    }
}
