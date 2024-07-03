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
        public static bool FilesAreEqual_Hash(string file1, string file2)
        {
            return FilesAreEqual_Hash(new FileInfo(file1), new FileInfo(file2));
        }

        public static bool FilesAreEqual_Hash(FileInfo file1, FileInfo file2)
        {
            byte[] file1Hash = SHA256.Create().ComputeHash(file1.OpenRead());
            byte[] file2Hash = SHA256.Create().ComputeHash(file2.OpenRead());

            for(int i = 0; i < file1Hash.Length; i++)
            {
                if (file1Hash[i] != file2Hash[i])
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
