using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azsync;

public record Login(string Tenant, string Client, string Secret) : ICommand { }

public class LoginHandler : IAsyncCommandHandler<Login>
{
    const string AZURE_REQUEST_CONTEXT_SCOPE = "https://graph.microsoft.com/.default";

    private readonly IConfigurationSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(IConfigurationSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Login command)
    {
        if (await IsLoggedInAsync())
        {
            Console.WriteLine("You are already logged in. You must first log out to update or change your credentials.");
            return;
        }

        await AuthenticateAsync(new ClientSecretCredential(tenantId: command.Tenant, clientId: command.Client, clientSecret: command.Secret));

        var settings = new[]
        {
            new ConfigurationSetting("tenantId", command.Tenant),
            new ConfigurationSetting("clientId", command.Client),
            new ConfigurationSetting("secret", command.Secret),
            new ConfigurationSetting("AUTH", string.Empty),
        };

        _repository.AddRange(settings);

        _unitOfWork.SaveChanges();

        Console.WriteLine("Login successful.");
    }

    private async Task<bool> IsLoggedInAsync()
    {
        var authState = await _repository.FindAsync("AUTH");

        return authState is not null;
    }

    private static async Task AuthenticateAsync(ClientSecretCredential credential)
    {
        try
        {
            await credential.GetTokenAsync(new TokenRequestContext(new[] { AZURE_REQUEST_CONTEXT_SCOPE }));
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS900023"))
        {
            Console.WriteLine("Invalid tenant identifier.");
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("ClientId"))
        {
            Console.WriteLine("Invalid client identifier.");
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS7000215"))
        {
            Console.WriteLine("Invalid client secret.");
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS1002012"))
        {
            Console.WriteLine("Invalid scope.");
        }

        catch (AuthenticationFailedException)
        {
            Console.WriteLine("Unexpected error during authentication. Please check your network connection.");
        }
    }
}
