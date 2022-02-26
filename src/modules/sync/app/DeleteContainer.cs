using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azpush;

public record DeleteContainer(string Name) : ICommand { }

public class DeleteContainerHandler : IAsyncCommandHandler<DeleteContainer>
{
    private readonly IAzureContainerRepository _containers;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContainerHandler(IAzureContainerRepository containers, IUnitOfWork unitOfWork)
    {
        _containers = containers;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteContainer command)
    {
        var container = await _containers.GetByNameAsync(command.Name);

        if (container is null)
        {
            Console.WriteLine($"Could not find container '{command.Name}'.");
            return;
        }

        _containers.Remove(container);
        
        await _unitOfWork.SaveChangesAsync();
    }
}
