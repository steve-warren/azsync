namespace azpush;

public record AuthenticationResult : Enumeration
{
    public static readonly AuthenticationResult OK = new() { Name = nameof(OK) };
    public static readonly AuthenticationResult InvalidTenantIdentifier = new() { Name = nameof(InvalidTenantIdentifier) };
    public static readonly AuthenticationResult InvalidClientIdentifier = new() { Name = nameof(InvalidClientIdentifier) };
    public static readonly AuthenticationResult InvalidClientSecret = new() { Name = nameof(InvalidClientSecret) };
    public static readonly AuthenticationResult InvalidScope = new() { Name = nameof(InvalidScope) };
    public static readonly AuthenticationResult CredentialAlreadyExists = new() { Name = nameof(CredentialAlreadyExists) };
    public static readonly AuthenticationResult UnknownError = new() { Name = nameof(UnknownError) };
}
