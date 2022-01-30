using azsync;
using Microsoft.Extensions.CommandLineUtils;

var app = new CommandLineApplication();
app.Name = "azsync";
app.HelpOption("-?|-h|--help");

app.Command("sync", (command) =>
{
    command.Description = "";
    command.HelpOption("-?|-h|--help");

    var locationArgument = command.Argument("[location]",
                                "Where the ninja should hide.");

    command.OnExecute(() =>
        {
            try
            {        
                using var context = new SyncDbContext();
                
                var c1 = new CreateLocalFileSnapshot("/Users/stevewarren/src/azsync", int.MaxValue);
                var h1 = new CreateLocalFileSnapshotHandler(
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

app.Command("auth", (command) =>
{
    command.OnExecute(() =>
    {
        return 0;
    });
});

app.OnExecute(() =>
{
    Console.WriteLine("auth");

    return 0;
});

app.Execute(args);