namespace azsync;

public interface IUnitOfWork
{
    void SaveChanges();
}