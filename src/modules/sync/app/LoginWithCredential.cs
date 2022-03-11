using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace azpush;

public record LoginWithCredential(string Name, string Tenant, string Client, string Secret) : ICommand { }

public class LoginWithCredentialHandler : IAsyncCommandHandler<LoginWithCredential>
{
    const string AZURE_REQUEST_CONTEXT_SCOPE = "https://graph.microsoft.com/.default";

    private readonly IAzureCredentialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringProtector _protector;

    public LoginWithCredentialHandler(IAzureCredentialRepository repository, IUnitOfWork unitOfWork, IStringProtector protector)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _protector = protector;
    }

    public async Task Handle(LoginWithCredential command)
    {
        AuthenticationResult authenticationResult = await AuthenticateAsync(new ClientSecretCredential(tenantId: command.Tenant, clientId: command.Client, clientSecret: command.Secret));

        if (authenticationResult == AuthenticationResult.OK)
        {
            var credentials = await _repository.GetByNameAsync(command.Name);

            if (credentials is not null)
            {
                credentials.Client = command.Client;
                credentials.Tenant = command.Tenant;
                credentials.SetSecret(command.Secret, _protector);
            }

            else
            {
                credentials = new AzureCredential(name: command.Name, tenant: command.Tenant, client: command.Client);
                credentials.SetSecret(command.Secret, _protector);
                _repository.Add(credentials);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        else
        {
            DisplayErrorMessage(authenticationResult);
        }
    }

    private static void DisplayErrorMessage(AuthenticationResult authenticationResult)
    {
        if (authenticationResult == AuthenticationResult.InvalidClientIdentifier) Console.WriteLine("Invalid client identifier.");
        else if (authenticationResult == AuthenticationResult.InvalidClientSecret) Console.WriteLine("Invalid client secret.");
        else if (authenticationResult == AuthenticationResult.InvalidScope) Console.WriteLine("Invalid scope.");
        else if (authenticationResult == AuthenticationResult.InvalidTenantIdentifier) Console.WriteLine("Invalid tenant identifier.");
        else if (authenticationResult == AuthenticationResult.UnknownError) Console.WriteLine("Unknown error. Please check your network and firewall settings.");
    }

    private static async Task<AuthenticationResult> AuthenticateAsync(ClientSecretCredential credential)
    {
        try
        {
            await credential.GetTokenAsync(new TokenRequestContext(new[] { AZURE_REQUEST_CONTEXT_SCOPE }));
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS900023"))
        {
            return AuthenticationResult.InvalidTenantIdentifier;
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("ClientId"))
        {
            return AuthenticationResult.InvalidClientIdentifier;
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS7000215"))
        {
            return AuthenticationResult.InvalidClientSecret;
        }

        catch (AuthenticationFailedException ex) when (ex.Message.Contains("AADSTS1002012"))
        {
            return AuthenticationResult.InvalidScope;
        }

        catch (AuthenticationFailedException)
        {
            return AuthenticationResult.UnknownError;
        }

        return AuthenticationResult.OK;
    }
}
