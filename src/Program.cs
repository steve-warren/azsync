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
                return 0;
            });
    });

app.OnExecute(() => {
        var c = new CaptureLocalFiles("/Users/stevewarren/src", int.MaxValue);

        var w = System.Diagnostics.Stopwatch.StartNew();
        using var context = new SyncDbContext();
        var handler = new CaptureLocalFilesHandler(
            new FileSystem(new Md5HashAlgorithm()),
            new SyncUnitOfWork(context),
            new LocalFileRepository(context));

        try
        {        
            handler.Handle(c);

            var f = new LocalFileRepository(context).GetNewLocalFiles().Count();

            Console.WriteLine(f);
        }

        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return 0;
    });

app.Execute(args);