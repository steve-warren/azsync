using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azsync;

public record DeleteCredential(string Name) : ICommand { }

public class DeleteCredentialHandler : IAsyncCommandHandler<DeleteCredential>
{
    private readonly IAzureCredentialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCredentialHandler(IAzureCredentialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCredential command)
    {
        var credential = await _repository.GetByNameAsync(command.Name);

        if (credential is null) return;

        _repository.Remove(credential);

        await _unitOfWork.SaveChangesAsync();
    }
}
