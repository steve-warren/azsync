using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azsync;

public record RemoveCredential(string Name) : ICommand { }

public class RemoveCredentialHandler : IAsyncCommandHandler<RemoveCredential>
{
    private readonly IAzureCredentialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCredentialHandler(IAzureCredentialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveCredential command)
    {
        var credential = await _repository.GetByNameAsync(command.Name);

        if (credential is null) return;

        _repository.Remove(credential);

        await _unitOfWork.SaveChangesAsync();
    }
}
