using System.Text;
using azpush;
using Microsoft.Extensions.CommandLineUtils;

Console.OutputEncoding = Encoding.UTF8;

var app = new CommandLineApplication();
app.Name = "azpush";
app.HelpOption("-?|-h|--help");

app.Command("push", (command) =>
{
    command.HelpOption("-?|-h|--help");

    command.OnExecute(async () =>
        {
            var context = new SyncDbContext();
            var handler = new PushHandler(new FileSystem(new Md5HashAlgorithm()), context, new LocalFileInfoCache(context), new BlobFileRepository(context));

            await handler.Handle(new Push());
            return 0;
        });
});

app.Command("set", (command) =>
{
    command.HelpOption("-?|-h|--help");

    command.Command("credential", (command) =>
    {
        var name = command.Option("-n|--name <name>", "A given name for the credentials.", CommandOptionType.SingleValue);
        var tenantOption = command.Option("-t|--tenant <tenantId>", "The Azure Active Directory tenant (directory) Id of the service principal.", CommandOptionType.SingleValue);
        var clientOption = command.Option("-c|--client <clientId>", "The client (application) Id of the service principal.", CommandOptionType.SingleValue);
        var clientSecret = command.Option("-s|--secret <secret>", "A client secret that was generated for the App Registration used to authenticate the client.", CommandOptionType.SingleValue);

        command.Description = "Log in to Azure.";

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            var command = new LoginWithCredential(Name: name.Value(), Tenant: tenantOption.Value(), Client: clientOption.Value(), Secret: clientSecret.Value());
            using var context = new SyncDbContext();
            
            var handler = new LoginWithCredentialHandler(new AzureCredentialRepository(context), context, new WindowsStringProtector());
            await handler.Handle(command);

            return 0;
        });
    });
});

app.Command("add", (command) =>
{
    command.HelpOption("-?|-h|--help");

    command.Command("path", (command) =>
    {
        var pathArgument = command.Argument("[path]", "The glob, file, or directory path which will be copied to blob storage.");
        var containerUrl = command.Argument("[containerUrl]", "The url of the blob storage container to place the blob files. The url may contain a virtual directory path.");
        var credential = command.Argument("[credential]", "The name of the credentials used to authenticate with this blob storage container.");
        var remoteFileName = command.Option("-n|--name <BLOB>", "The name for the blob file if path is a file.", CommandOptionType.SingleValue);
        var includeTimestamp = command.Option("-t|--timestamp <TIMESTAMP>", "Append a UTC-formatted timestamp to the blob file name.", CommandOptionType.NoValue);

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            var blobName = remoteFileName.HasValue() ? remoteFileName.Value() : null;

            using var context = new SyncDbContext();
            var command = new AddPath(Path: pathArgument.Value, CredentialName: credential.Value, ContainerUrl: containerUrl.Value, BlobName: blobName, IncludeTimestamp: includeTimestamp.HasValue());
            var handler = new AddPathHandler(new FileSystem(new Md5HashAlgorithm()), context, new AzureCredentialRepository(context));
            await handler.Handle(command);
            return 0;
        });
    });
});

app.Command("list", (command) =>
{
    command.Command("credentials", (command) =>
    {
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            var handler = new ListCredentialsHandler(new AzureCredentialRepository(context));
            await handler.Handle(new ListCredentials());

            return 0;
        });
    });

    command.Command("paths", (command) =>
    {
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            await foreach(var path in context.LocalPaths.AsAsyncEnumerable())
                Console.WriteLine(path.Path);

            return 0;
        });
    });
});

app.Command("remove", (command) =>
{
    command.Command("credential", (command) =>
    {
        command.HelpOption("-?|-h|--help");
        var credentialName = command.Argument("[name]", "The name of the credential to delete.");
        
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            var handler = new DeleteCredentialHandler(new AzureCredentialRepository(context), context);
            await handler.Handle(new DeleteCredential(Name: credentialName.Value));

            return 0;
        });
    });

    command.Command("path", (command) =>
    {
        var pathArgument = command.Argument("[path]", "The name of the path to delete.");
        
        command.OnExecute(async () =>
        {
            var handler = new RemovePathHandler(new SyncDbContext());
            await handler.Handle(new RemovePath(Path: pathArgument.Value));

            return 0;
        });
    });
});

try
{
    app.Execute(args);
}

catch(Exception ex)
{
    Console.WriteLine("Invalid command, argument, or transient error. " + ex.Message);
    return 1;
}

return 0;