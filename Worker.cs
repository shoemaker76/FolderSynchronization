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
        string sourceDirLog = $"Source Folder: {sourceDir}";
        _logger.LogInformation(sourceDirLog);
        string replicaDirLog = $"Replica Folder: {replicaDir}";
        _logger.LogInformation(replicaDirLog);
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting the synchronization process...");

            List<Task> allTasks = [];

            Dictionary<string, byte[]> sourceFiles = FilesUtilities.GetFiles(sourceDir).ToDictionary(x => Path.GetRelativePath(sourceDir, x), FilesUtilities.GetFileContentsHash);
            Dictionary<string, byte[]> replicaFiles = FilesUtilities.GetFiles(replicaDir).ToDictionary(x => Path.GetRelativePath(replicaDir, x), FilesUtilities.GetFileContentsHash);

            foreach(var file in replicaFiles.Keys)
            {
                if (!sourceFiles.ContainsKey(file))
                {
                    File.Delete(Path.Join(replicaDir, file));
                    replicaFiles.Remove(file);
                    LoggingHelper.LogFileDeletion(_logger, replicaDir, file);
                }
            }

            FilesUtilities.DeleteDirectories(_logger, sourceDir, replicaDir);

            FilesUtilities.CopyDirectories(_logger, sourceDir, replicaDir);

            foreach (var file in sourceFiles.Keys)
            {
                if (replicaFiles.ContainsKey(file))
                {
                    if (FilesUtilities.HashesAreEqual(sourceFiles[file], replicaFiles[file]))
                    {
                        continue;
                    }
                    else
                    {
                        File.Delete(Path.Join(replicaDir, file));
                        LoggingHelper.LogFileDeletion(_logger, replicaDir, file);
                    }
                }
                allTasks.Add(Task.Run(() => File.Copy(Path.Join(sourceDir, file), Path.Join(replicaDir, file)), stoppingToken)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            LoggingHelper.LogFileCopy(_logger, sourceDir, replicaDir, file);
                        }
                    }, stoppingToken)
                );
            }

            await Task.Delay(int.Parse(_configuration["SynchronizationInterval"]!), stoppingToken);
            Task.WaitAll(allTasks.ToArray(), stoppingToken);
            _logger.LogInformation("Synchronization finished.");
        }
    }
}
