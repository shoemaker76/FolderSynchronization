using System.Security.Cryptography;

namespace FolderSynchronization.Helpers
{
    public static class FilesUtilities
    {
        public static byte[] GetFileContentsHash(string file) => GetFileContentsHash(new FileInfo(file));
        public static byte[] GetFileContentsHash(FileInfo file)
        {
            using var fileStream = file.OpenRead();
            return SHA256.Create().ComputeHash(fileStream);
        }

        public static bool HashesAreEqual(byte[] hash1, byte[] hash2)
        {
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static void CopyDirectories(ILogger logger, string sourceDir, string replicaDir)
        {
            CopyDirectories(logger, new DirectoryInfo(sourceDir), new DirectoryInfo(replicaDir));
        }

        public static void CopyDirectories(ILogger logger, DirectoryInfo sourceDirInfo, DirectoryInfo replicaDirInfo)
        {
            DirectoryInfo[] sourceSubDirInfo = sourceDirInfo.GetDirectories();
            DirectoryInfo[] replicaSubDirInfo = replicaDirInfo.GetDirectories();

            foreach (var subDirInfo in sourceSubDirInfo)
            {
                if (!replicaSubDirInfo.Select(x => x.Name).ToList().Contains(subDirInfo.Name))
                {
                    var replicaNewDirectoryInfo = Directory.CreateDirectory(Path.Join(replicaDirInfo.FullName, subDirInfo.Name));
                    LoggingHelper.LogDirCreation(logger, replicaDirInfo.FullName, subDirInfo.Name);
                    CopyDirectories(logger,subDirInfo, replicaNewDirectoryInfo);
                }
            }
        }

        public static void DeleteDirectories(ILogger logger, string sourceDir, string replicaDir)
        {
            DeleteDirectories(logger, new DirectoryInfo(sourceDir), new DirectoryInfo(replicaDir));
        }

        public static void DeleteDirectories(ILogger logger, DirectoryInfo sourceDirInfo, DirectoryInfo replicaDirInfo)
        {
            DirectoryInfo[] sourceSubDirInfo = sourceDirInfo.GetDirectories();
            DirectoryInfo[] replicaSubDirInfo = replicaDirInfo.GetDirectories();

            foreach (var subDirInfo in replicaSubDirInfo)
            {
                if (sourceSubDirInfo.Select(x => x.Name).ToList().Contains(subDirInfo.Name))
                {
                    DeleteDirectories(logger, new DirectoryInfo(Path.Join(sourceDirInfo.FullName, subDirInfo.Name)), subDirInfo);
                }
                else
                {
                    Directory.Delete(Path.Join(replicaDirInfo.FullName, subDirInfo.Name));
                    LoggingHelper.LogDirDeletion(logger, replicaDirInfo.FullName, subDirInfo.Name);
                }
            }
        }

        public static string[] GetFiles(string directory) => Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
    }
}
