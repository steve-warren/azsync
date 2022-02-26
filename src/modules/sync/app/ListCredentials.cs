using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azpush;

public record ListCredentials() : ICommand { }

public class ListCredentialsHandler : IAsyncCommandHandler<ListCredentials>
{
    private readonly IAzureCredentialRepository _repository;

    public ListCredentialsHandler(IAzureCredentialRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ListCredentials command)
    {
        await foreach(var credential in _repository.ListAsync())
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { credential.Name, credential.Tenant, credential.Client }));
    }
}
