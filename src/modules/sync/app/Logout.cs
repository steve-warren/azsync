using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azsync;

public record Logout() : ICommand { }

public class LogoutHandler : IAsyncCommandHandler<Logout>
{
    private readonly IConfigurationSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(IConfigurationSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Logout command)
    {
        var settings = new[]
        {
            await _repository.FindAsync("tenantId"),
            await _repository.FindAsync("clientId"),
            await _repository.FindAsync("secret"),
            await _repository.FindAsync("AUTH")
        };

        _repository.RemoveRange(settings);

        _unitOfWork.SaveChanges();

        Console.WriteLine("Logout successful.");
    }
}
