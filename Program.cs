using FolderSynchronization;
using Serilog;


string usageHelper = "Usage: dotnet run SourceDirectory=[source-directory] ReplicaDirectory=[replica-directory] LogsFilePath=[logs-file-path] SynchronizationInterval=[synchronization-interval]\n\n"
                    + "source-directory:\n\tThe path of the directory to be copied to the replica folder.\n"
                    + "replica-directory:\n\tThe path of the directory to where the contents of the source folder will be copied to.\n"
                    + "logs-file-path:\n\tThe path of the logs file.\n"
                    + "synchronization-interval:\n\tThe interval in which the synchonization will occur in milliseconds.\n";

try
{
    if(args.Length < 4)
    {
        Console.WriteLine(usageHelper);
        return 1;
    }
    //TODO: Parse LogsFilePath Arg
    var loggerFilePath = args[2];

    var builder = Host.CreateDefaultBuilder(args);
    builder.UseSerilog(new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(args[2])
            .CreateLogger())

        .ConfigureServices(services =>
            services.AddHostedService<Worker>());

    var app = builder.Build();
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    Console.WriteLine(usageHelper);
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}