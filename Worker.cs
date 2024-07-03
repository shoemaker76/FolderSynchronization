using FolderSynchronization.Helpers;

namespace FolderSynchronization;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string sourceDir = _configuration["SourceDirectory"]!;
        string replicaDir = _configuration["ReplicaDirectory"]!;
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting the synchronization process...");
            _logger.LogInformation($"Source Folder: {sourceDir}");
            _logger.LogInformation($"Replica Folder: {replicaDir}");

            Task[] allTasks = [];

            FilesUtilities.CopyDirectories(sourceDir, replicaDir);

            string[] sourceDirFiles = FilesUtilities.GetFiles(sourceDir);
            string[] replicaDirFiles = FilesUtilities.GetFiles(replicaDir);
            foreach (var file in sourceDirFiles)
            {
                allTasks.Append(Task.Run(() => File.Copy(file, Path.Join(replicaDir, Path.GetRelativePath(sourceDir, file))), stoppingToken)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            _logger.LogInformation($"Copied \"{Path.GetFileName(file)}\" from {sourceDir} to {replicaDir}.");
                        }
                    }, stoppingToken)
                );
            }

            await Task.Delay(int.Parse(_configuration["SynchronizationInterval"]!), stoppingToken);
            Task.WaitAll(allTasks, stoppingToken);
            _logger.LogInformation("Synchronization finished.");
        }
    }
}
