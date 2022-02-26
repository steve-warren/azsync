namespace azpush;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}