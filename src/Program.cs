using System.Diagnostics;
using System.Text;
using azsync;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;

Console.OutputEncoding = Encoding.UTF8;

var app = new CommandLineApplication();
app.Name = "azpush";
app.HelpOption("-?|-h|--help");

app.Command("push", (command) =>
{
    command.Description = "";
    command.HelpOption("-?|-h|--help");

    command.OnExecute(async () =>
        {
            var hash = new Md5HashAlgorithm();
            var fs = new FileSystem(hash);
            using var context = new SyncDbContext();
            var localFiles = new LocalFileRepository(context);
            var _syncFiles = new SyncFileRepository(context);
            
            foreach(var path in await context.LocalPaths.ToListAsync())
            {
                Console.WriteLine($"Looking for changes to {path.PathType.ToLowerInvariant()} '{path.Path}'...");
                var localFilesPendingUpdate = Enumerable.Empty<LocalFile>();

                try
                {
                    localFilesPendingUpdate = path.GetLocalFiles(fs);
                }

                catch(Exception)
                {
                    Console.WriteLine("Unable to read from file system. Ensure this process has the correct permissions set.");
                    return 1;
                }

                localFiles.ReplaceAll(localFilesPendingUpdate);

                var container = await context.AzureContainers.FirstOrDefaultAsync(c => c.Id == path.ContainerId);
                var credentials = await context.AzureCredentials.FirstOrDefaultAsync(c => c.Id == container.CredentialId);

                var serviceClient = new BlobServiceClient(new Uri(container.ContainerUrl), new ClientSecretCredential(tenantId: credentials.Tenant, clientId: credentials.Client, clientSecret: credentials.Secret));
                
                var containerClient = serviceClient.GetBlobContainerClient(container.Name);

                foreach(var fileMetadata in await localFiles.GetNew())
                {
                    var file = new SyncFile(name: fileMetadata.Name, localFilePath: fileMetadata.Path, localFilePathHash: fileMetadata.PathHash, lastModified: fileMetadata.LastModified, fileSizeInBytes: fileMetadata.FileSizeInBytes, containerId: fileMetadata.ContainerId, localPathId: fileMetadata.LocalPathId);

                    try
                    {
                        using var fileStream = new FileStream(path: file.LocalFilePath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);

                        file.SetContentHash(await new Md5HashAlgorithm().ComputeHashAsync(fileStream));
                        fileStream.Seek(0, SeekOrigin.Begin);

                        Console.Write($"new: {file.Name} ({file.FileSizeInBytes} bytes) ...");

                        var blobClient = containerClient.GetBlobClient(file.Name);
                        var blob = await blobClient.UploadAsync(fileStream, overwrite: true);
                    
                        file.Upload(Convert.ToBase64String(blob.Value.ContentHash), DateTimeOffset.Now);
                    }

                    catch(Exception)
                    {
                        file.Error();
                    }

                    _syncFiles.Add(file);

                    await context.SaveChangesAsync();

                    if (file.State == "Error")
                        Console.WriteLine("ERR 😬");

                    if (file.State == "Uploaded")
                        Console.WriteLine("OK. 🙌");
                }

                var updatedFiles = await localFiles.GetModified().ToListAsync();

                foreach(var fileMetadata in updatedFiles)
                {
                    var file = context.SyncFiles.First(sf => sf.LocalFilePathHash == fileMetadata.PathHash);

                    file.LastModified = fileMetadata.LastModified;
                    file.FileSizeInBytes = fileMetadata.FileSizeInBytes;

                    try
                    {
                        using var fileStream = new FileStream(path: file.LocalFilePath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);

                        file.SetContentHash(await new Md5HashAlgorithm().ComputeHashAsync(fileStream));
                        fileStream.Seek(0, SeekOrigin.Begin);

                        Console.Write($"modified: {fileMetadata.Name} ({file.FileSizeInBytes} bytes) ...");

                        var blobClient = containerClient.GetBlobClient(file.Name);

                        var blob = await blobClient.UploadAsync(fileStream, overwrite: true);
                    
                        file.Upload(Convert.ToBase64String(blob.Value.ContentHash), DateTimeOffset.Now);
                    }

                    catch(Exception ex)
                    {
                        file.Error();
                        Console.WriteLine(ex.Message);
                    }

                    await context.SaveChangesAsync();

                    if (file.State == "Error")
                        Console.WriteLine("ERR 😬");

                    if (file.State == "Uploaded")
                        Console.WriteLine("OK. 🙌");
                }

                
                var deletedFiles = await _syncFiles.GetDeleted().ToListAsync();

                foreach(var file in deletedFiles)
                {
                    Console.Write($"deleted: {file.Name} ({file.FileSizeInBytes} bytes) ...");

                    context.SyncFiles.Remove(file);
                    await context.SaveChangesAsync();

                    Console.WriteLine("OK. 🙌");
                }
            }

            return 0;
        });
});

app.Command("add", (command) =>
{
    command.Command("credential", (command) =>
    {
        var tenantOption = command.Option("-t|--tenant <tenantId>", "The Azure Active Directory tenant (directory) Id of the service principal.", CommandOptionType.SingleValue);
        var clientOption = command.Option("-c|--client <clientId>", "The client (application) Id of the service principal.", CommandOptionType.SingleValue);
        var clientSecret = command.Option("-s|--secret <secret>", "A client secret that was generated for the App Registration used to authenticate the client.", CommandOptionType.SingleValue);
        var name = command.Option("-n|--name <name>", "A given name for the credentials.", CommandOptionType.SingleValue);

        command.Description = "Log in to Azure.";

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            var command = new LoginWithCredential(Name: name.Value(), Tenant: tenantOption.Value(), Client: clientOption.Value(), Secret: clientSecret.Value());
            using var context = new SyncDbContext();
            
            var handler = new LoginWithCredentialHandler(new AzureCredentialRepository(context), context);
            await handler.Handle(command);

            return 0;
        });
    });

    command.Command("container", (command) =>
    {
        var url = command.Argument("[blobStorageContainerUrl]", "The url to the blob storage container.");
        var name = command.Argument("[name]", "A given name for the blob storage container.");
        var credential = command.Argument("[credential]", "The name of the credentials used to authenticate with this blob storage container.");

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();
            var command = new AddAzureContainer(ContainerUrl: url.Value, Name: name.Value, CredentialName: credential.Value);
            var handler = new AddAzureContainerHandler(new AzureCredentialRepository(context), new AzureContainerRepository(context), context);
            await handler.Handle(command);
            return 0;
        });
    });

    command.Command("path", (command) =>
    {
        var pathArgument = command.Argument("[path]", "The glob, file, or directory path.");
        var containerName = command.Argument("[containerName]", "The name of the container the file will be copied.");

        command.HelpOption("-?|-h|--help");

        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();
            var command = new AddPath(Path: pathArgument.Value, ContainerName: containerName.Value);
            var handler = new AddPathHandler(new FileSystem(new Md5HashAlgorithm()), context);
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

    command.Command("containers", (command) =>
    {
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            var handler = new ListAzureContainersHandler(new AzureContainerRepository(context));
            await handler.Handle(new ListAzureContainers());

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

    command.Command("container", (command) =>
    {
        command.HelpOption("-?|-h|--help");
        var containerName = command.Argument("[name]", "The name of the container to delete.");
        
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            var handler = new DeleteContainerHandler(new AzureContainerRepository(context), context);
            await handler.Handle(new DeleteContainer(Name: containerName.Value));

            return 0;
        });
    });

    command.Command("path", (command) =>
    {
        var pathArgument = command.Argument("[path]", "The name of the path to delete.");
        
        command.OnExecute(async () =>
        {
            using var context = new SyncDbContext();

            var path = context.LocalPaths.FirstOrDefault(p => p.Path == pathArgument.Value);

            if (path is null)
            {
                Console.WriteLine("Path not found.");
                return 0;
            }

            context.LocalPaths.Remove(path);
            await context.SaveChangesAsync();
            Console.WriteLine("Path removed.");

            return 0;
        });
    });
});

app.Execute(args);
