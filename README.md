# Folder Synchronization app written in .Net 8

**Usage:** 

dotnet run SourceDirectory=[source-directory] ReplicaDirectory=[replica-directory] LogsFilePath=[logs-file-path] SynchronizationInterval=[synchronization-interval]

source-directory:
        The path of the directory to be copied to the replica folder.
replica-directory:
        The path of the directory to where the contents of the source folder will be copied to.
logs-file-path:
        The path of the logs file.
synchronization-interval:
        The interval in which the synchonization will occur in milliseconds.
