using FolderSynchronization.Helpers;

namespace FolderSynchronization.Tests
{
    [TestFixture]
    public class Tests : TestsBase
    {
        [TearDown]
        public void Teardown() 
        {
            string[] sourcefiles = FilesUtilities.GetFiles(sourceTestDir);
            string[] replicafiles = FilesUtilities.GetFiles(replicaTestDir);
            foreach (var file in sourcefiles.Concat(replicafiles))
            {
                File.Delete(file);
            }

            DeleteDirectories(new DirectoryInfo(sourceTestDir));
            DeleteDirectories(new DirectoryInfo(replicaTestDir));
        }

        [Test]
        public async Task Test_One_Folder()
        {
            //Add folder to sourceDir
            Directory.CreateDirectory(Path.Join(sourceTestDir, "a"));

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_One_File()
        {
            //Add file to sourceDir
            var filename = "file1.txt";
            File.WriteAllText(Path.Join(sourceTestDir, filename), "test");

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_Recursive_Folders()
        {
            //Add folders to sourceDir
            Directory.CreateDirectory(Path.Join(sourceTestDir, "a"));
            var b = Directory.CreateDirectory(Path.Join(sourceTestDir, "b"));
            b.CreateSubdirectory("bb"); 
            var c = Directory.CreateDirectory(Path.Join(sourceTestDir, "c"));
            var cc = c.CreateSubdirectory("cc");
            cc.CreateSubdirectory("ccc");

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_Recursive_Folders_With_Files()
        {
            //Add folders and files to sourceDir
            Directory.CreateDirectory(Path.Join(sourceTestDir, "a"));
            string filename = "fileA.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "a", filename), "test1");

            var b = Directory.CreateDirectory(Path.Join(sourceTestDir, "b"));
            filename = "fileB.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "b", filename), "test2");
            b.CreateSubdirectory("bb");
            filename = "fileBB.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "b", "bb", filename), "test3");
            var c = Directory.CreateDirectory(Path.Join(sourceTestDir, "c"));
            filename = "fileC.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "c", filename), "test4");
            var cc = c.CreateSubdirectory("cc");
            filename = "fileCC.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "c", "cc", filename), "test5");
            cc.CreateSubdirectory("ccc");
            filename = "fileCCC.txt";
            File.WriteAllText(Path.Join(sourceTestDir, "c", "cc", "ccc", filename), "test6");

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_File_Content_Not_Same()
        {
            string filename = "file.txt";
            File.WriteAllText(Path.Join(sourceTestDir, filename), "source file");
            filename = "file.txt";
            File.WriteAllText(Path.Join(replicaTestDir, filename), "replica file");

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_Replica_Folder_Has_Folder()
        {
            //Add extra folder
            Directory.CreateDirectory(Path.Join(replicaTestDir, "extra"));

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }

        [Test]
        public async Task Test_Replica_Folder_Has_File()
        {
            //Add extra file
            var filename = "file.txt";
            File.WriteAllText(Path.Join(replicaTestDir, filename), "test");

            //Start service
            await StartSynchroniczationServiceAsync();

            //Assertions
            AssertFolderContents();
        }
    }
}