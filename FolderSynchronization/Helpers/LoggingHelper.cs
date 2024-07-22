namespace FolderSynchronization.Helpers
{
    internal static class LoggingHelper
    {
        public static void LogFileCopy(ILogger logger, string sourceDir, string replicaDir, string file)
        {
            string message = $"Copied \"{Path.GetFileName(file)}\" from {sourceDir} to {replicaDir}.";
            logger.LogInformation(message);
        }

        public static void LogFileDeletion(ILogger logger, string replicaDir, string file) 
        {
            string message = $"Deleted \"{Path.GetFileName(file)}\" from {replicaDir}.";
            logger.LogInformation(message);
        }

        public static void LogDirCreation(ILogger logger, string replicaDir, string dir) 
        {
            string message = $"Directory \"{Path.GetFileName(dir)}\" created on {replicaDir}.";
            logger.LogInformation(message);
        }

        public static void LogDirDeletion(ILogger logger, string replicaDir, string dir) 
        {
            string message = $"Deleted \"{Path.GetFileName(dir)}\" from {replicaDir}.";
            logger.LogInformation(message);
        }
    }
}
