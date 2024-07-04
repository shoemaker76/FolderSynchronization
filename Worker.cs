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
        _logger.LogInformation($"Source Folder: {sourceDir}");
        _logger.LogInformation($"Replica Folder: {replicaDir}");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting the synchronization process...");

            Task[] allTasks = [];

            FilesUtilities.CopyDirectories(sourceDir, replicaDir);

            Dictionary<string, byte[]> sourceFiles = FilesUtilities.GetFiles(sourceDir).ToDictionary(x => Path.GetRelativePath(sourceDir, x), FilesUtilities.GetFileContentsHash);
            Dictionary<string, byte[]> replicaFiles = FilesUtilities.GetFiles(replicaDir).ToDictionary(x => Path.GetRelativePath(replicaDir, x), FilesUtilities.GetFileContentsHash);

            string[] replicaDirFiles = FilesUtilities.GetFiles(replicaDir);
            foreach (var file in sourceFiles.Keys)
            {
                //TODO: check if move is better is case contents are same
                if (replicaFiles.ContainsKey(file))
                {
                    if (FilesUtilities.HashesAreEqual(sourceFiles[file], replicaFiles[file]))
                    {
                        continue;
                    }
                    else
                    {
                        _logger.LogInformation($"Deleted \"{Path.GetFileName(file)}\" from {replicaDir}.");
                        File.Delete(Path.Join(replicaDir, file));
                    }
                }
                allTasks.Append(Task.Run(() => File.Copy(Path.Join(sourceDir, file), Path.Join(replicaDir, file)), stoppingToken)
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
