using FluentAssertions;
using FolderSynchronization.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FolderSynchronization.Tests
{
    [SetUpFixture]
    public class TestsBase
    {
        protected const string sourceTestDir = "sourceFolderUnitTests";
        protected const string replicaTestDir = "replicaFolderUnitTests";

        [OneTimeSetUp]
        public void Init()
        {
            Directory.CreateDirectory(sourceTestDir);
            Directory.CreateDirectory(replicaTestDir);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(sourceTestDir);
            Directory.Delete(replicaTestDir);
        }

        protected void DeleteDirectories(DirectoryInfo path)
        {
            DirectoryInfo[] pathDirInfo = path.GetDirectories();
            foreach (var subDirInfo in pathDirInfo)
            {
                DeleteDirectories(subDirInfo);
                if (subDirInfo.GetDirectories().Length == 0)
                {
                    subDirInfo.Delete();
                }
            }

        }

        protected IEnumerable<string> GetFileSystemEntries(string path) => Directory.EnumerateFileSystemEntries(path, "*.*", SearchOption.AllDirectories);

        protected async Task StartSynchroniczationServiceAsync()
        {
            //Start service
            var commandLineArgs = $"SourceDirectory={sourceTestDir} ReplicaDirectory={replicaTestDir} LogsFilePath=null SynchronizationInterval=60000".Split();
            var builder = Host.CreateDefaultBuilder(commandLineArgs);
            builder.ConfigureLogging(c => c.AddConsole())
                .ConfigureServices(services =>
                    services.AddHostedService<Worker>());

            var app = builder.Build();

            await app.StartAsync(CancellationToken.None);
            //Give some time to invoke the methods under test
            await Task.Delay(1000);
            await app.StopAsync(CancellationToken.None);
        }

        protected void AssertFolderContents()
        {
            var sourceFileSystemEntries = GetFileSystemEntries(sourceTestDir).Select(x => Path.GetRelativePath(sourceTestDir, x)).ToArray();
            var replicaFileSystemEntries = GetFileSystemEntries(replicaTestDir).Select(x => Path.GetRelativePath(replicaTestDir, x)).ToArray();
            replicaFileSystemEntries.Should().HaveSameCount(sourceFileSystemEntries);
            replicaFileSystemEntries.Should().BeEquivalentTo(sourceFileSystemEntries);

            var files = FilesUtilities.GetFiles(sourceTestDir);
            foreach (var file in files)
            {
                var replicaFile = Path.Join(replicaTestDir, Path.GetRelativePath(sourceTestDir, file));
                var fileHash = FilesUtilities.GetFileContentsHash(file);
                var replicaHash = FilesUtilities.GetFileContentsHash(replicaFile);
                replicaHash.Should().BeEquivalentTo(fileHash);
            }
        }
    }
}
