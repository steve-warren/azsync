using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azsync;

public record ListAzureContainers() : ICommand { }

public class ListAzureContainersHandler : IAsyncCommandHandler<ListAzureContainers>
{
    private readonly IAzureContainerRepository _containers;

    public ListAzureContainersHandler(IAzureContainerRepository containers)
    {
        _containers = containers;
    }

    public async Task Handle(ListAzureContainers command)
    {
        await foreach(var container in _containers.ListAsync())
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { container.Name, container.ContainerUrl }));
    }
}
