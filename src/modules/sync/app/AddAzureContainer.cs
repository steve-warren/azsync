using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azpush;

public record AddAzureContainer(string ContainerUrl, string Name, string CredentialName) : ICommand { }

public class AddAzureContainerHandler : IAsyncCommandHandler<AddAzureContainer>
{
    private readonly IAzureCredentialRepository _credentials;
    private readonly IAzureContainerRepository _containers;
    private readonly IUnitOfWork _unitOfWork;

    public AddAzureContainerHandler(IAzureCredentialRepository credentials, IAzureContainerRepository containers, IUnitOfWork unitOfWork)
    {
        _credentials = credentials;
        _containers = containers;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddAzureContainer command)
    {
        var credential = await _credentials.GetByNameAsync(command.CredentialName);

        if (credential is null)
        {
            Console.WriteLine($"Cannot find credental '{command.CredentialName}'.");
            return;
        }

        if (await _containers.ExistsAsync(command.Name))
        {
            Console.WriteLine($"Container '{command.Name}' already exists.");
            return;
        }

        var container = new AzureContainer(command.Name, command.ContainerUrl, credential.Id);

        _containers.Add(container);

        await _unitOfWork.SaveChangesAsync();
    }
}
