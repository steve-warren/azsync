using System.Diagnostics;
using azsync;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.CommandLineUtils;

var app = new CommandLineApplication();
app.Name = "azsync";
app.HelpOption("-?|-h|--help");

app.Command("sync", (command) =>
{
    command.Description = "";
    command.HelpOption("-?|-h|--help");

    var locationArgument = command.Argument("[path]",
                                "An absolute or relative path of the directory to sync.");

    command.OnExecute(() =>
        {
            try
            {        
                using var context = new SyncDbContext();
                
                var c1 = new CaptureLocalDirectory(DirectoryPath: locationArgument.Value, SearchPattern: "*");
                var h1 = new CaptureLocalDirectoryHandler(
                    new FileSystem(new Md5HashAlgorithm()),
                    new LocalFileRepository(context));

                var c2 = new TrackNewFiles();
                var h2 = new TrackNewFilesHandler(new LocalFileRepository(context), new SyncFileRepository(context), context);

                h1.Handle(c1);
                h2.Handle(c2);
            }
        
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return 0;
        });
});

app.Command("add", (command) =>
{
    command.Command("remote", (command) =>
    {
        var tenantOption = command.Option("-t|--tenant <tenantId>", "The Azure Active Directory tenant (directory) Id of the service principal.", CommandOptionType.SingleValue);
        var clientOption = command.Option("-c|--client <clientId>", "The client (application) Id of the service principal.", CommandOptionType.SingleValue);
        var clientSecret = command.Option("-s|--secret <secret>", "A client secret that was generated for the App Registration used to authenticate the client.", CommandOptionType.SingleValue);
        var container = command.Option("-cn|--containerName <containerName>", "The name of the blob storage container.", CommandOptionType.SingleValue);
        var name = command.Option("-n|--name <name>", "The name or alias of this remote target.", CommandOptionType.SingleValue);

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            Console.WriteLine("add remote execute.");
            return 0;
        });
    });

    command.Command("path", (command) =>
    {
        command.HelpOption("-?|-h|--help");

        command.OnExecute(() =>
        {
            Console.WriteLine("add path execute.");
            return 0;
        });
    });
});

app.Command("login", (command) =>
{
    command.Description = "Log in to Azure.";

    var tenantOption = command.Option("-t|--tenant <tenantId>", "The Azure Active Directory tenant (directory) Id of the service principal.", CommandOptionType.SingleValue);
    var clientOption = command.Option("-c|--client <clientId>", "The client (application) Id of the service principal.", CommandOptionType.SingleValue);
    var clientSecret = command.Option("-s|--secret <secret>", "A client secret that was generated for the App Registration used to authenticate the client.", CommandOptionType.SingleValue);

    command.HelpOption("-?|-h|--help");

    command.OnExecute(async () =>
    {
        var command = new Login(Tenant: tenantOption.Value(), Client: clientOption.Value(), Secret: clientSecret.Value());
        using var context = new SyncDbContext();
        
        var handler = new LoginHandler(new ConfigurationSettingRepository(context), context);
        await handler.Handle(command);

        return 0;
    });
});

app.Command("logout", (command) =>
{
    command.Description = "Logs the application out of Azure.";

    command.OnExecute(async () =>
    {
        using var context = new SyncDbContext();
        var handler = new LogoutHandler(new ConfigurationSettingRepository(context), context);
        await handler.Handle(new Logout());

        return 0;
    });
});

app.OnExecute(async () =>
{
    var serviceClient = new BlobServiceClient(new Uri(""), null);

    var name = DateTime.Now.Millisecond.ToString();

    await serviceClient.CreateBlobContainerAsync(name);
    var containerClient = serviceClient.GetBlobContainerClient(name);

    using var context = new SyncDbContext();

    var watch = Stopwatch.StartNew();
    await foreach(var file in context.SyncFiles.AsAsyncEnumerable())
    {
        try
        {
            Console.WriteLine(file.Name);
            using var fileStream = new FileStream(path: file.LocalFilePath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);

            file.SetContentHash(await new Md5HashAlgorithm().ComputeHashAsync(fileStream));
            fileStream.Seek(0, SeekOrigin.Begin);

            var blob = await containerClient.UploadBlobAsync(file.LocalFilePath, fileStream);

            file.Upload(Convert.ToBase64String(blob.Value.ContentHash));

            await context.SaveChangesAsync();
        }

        catch (FileNotFoundException)
        {
            file.NotFound();
            
            await context.SaveChangesAsync();
        }
    }

    Console.WriteLine(watch.ElapsedMilliseconds);

    return -1;
});

app.Execute(args);
