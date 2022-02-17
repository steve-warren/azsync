namespace azsync;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}