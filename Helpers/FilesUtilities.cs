using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderSynchronization.Helpers
{
    internal class FilesUtilities
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

        public static void CopyDirectories(string sourceDir, string replicaDir)
        {
            CopyDirectories(new DirectoryInfo(sourceDir), new DirectoryInfo(replicaDir));
        }

        public static void CopyDirectories(DirectoryInfo sourceDirInfo, DirectoryInfo replicaDirInfo)
        {
            DirectoryInfo[] sourceSubDirInfo = sourceDirInfo.GetDirectories();
            DirectoryInfo[] replicaSubDirInfo = replicaDirInfo.GetDirectories();

            foreach (var subDirInfo in sourceSubDirInfo)
            {
                if (!replicaSubDirInfo.Select(x => x.Name).ToList().Contains(subDirInfo.Name))
                {
                    var replicaNewDirectoryInfo = Directory.CreateDirectory(Path.Join(replicaDirInfo.FullName, subDirInfo.Name));
                    CopyDirectories(subDirInfo, replicaNewDirectoryInfo);
                }
            }
            return;
        }

        public static string[] GetFiles(string directory) => Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
    }
}
