namespace azsync;

public interface ICommandHandler<T> where T : ICommand
{
    void Handle(T command);
}

public interface ICommandHandler<T,T1> where T : ICommand
{
    T1 Handle(T command);
}