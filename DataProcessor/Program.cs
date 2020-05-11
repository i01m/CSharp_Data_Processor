using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using static System.Console;

namespace DataProcessor
{
    class Program
    {
        private static ConcurrentDictionary<string, string> FilesToProcess =
                                    new ConcurrentDictionary<string, string>();

        //alternative approach is to use Cache option - look how to use it with Core app
        //pluralsight shows how to use it with.net framework only,with adding refernce to
        //System.Runtime.Caching using MemoryCache

        static void Main(string[] args)
        {
            WriteLine("Process command line options");

            //auto watch the dir

            var directoryToWatch = args[0];

            if (!Directory.Exists(directoryToWatch))
            {
                WriteLine($"ERROR: The directory {directoryToWatch} does not exist!");
            }
            else
            {
                WriteLine($"Watching directory {directoryToWatch} for changes.");

                ProcessExistingFiles(directoryToWatch);
                
                using (var inputFileWatcher = new FileSystemWatcher(directoryToWatch))
                using (var timer = new Timer(ProcessFiles, null, 0, 1000))
                {
                    inputFileWatcher.IncludeSubdirectories = false;
                    inputFileWatcher.InternalBufferSize = 32768; //32 kb
                    inputFileWatcher.Filter = "*.*";
                    inputFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

                    inputFileWatcher.Created += FileCreated;
                    inputFileWatcher.Changed += FileChanged;
                    inputFileWatcher.Deleted += FileDeleted;
                    inputFileWatcher.Renamed += FileRenamed;
                    inputFileWatcher.Error += WatcherError;

                    inputFileWatcher.EnableRaisingEvents = true;
                   
                  
                    ReadKey(); 
                }
            }
              WriteLine("Press Enter to quit.");
           


            /* This was for manually run the console app
             * now auto watch dir is added and commented this section
             * 
            var command = args[0];

            if (command == "--file")
            {
                var filePath = args[1];
                WriteLine($"Single file {filePath} is selected");
                ProcessSingleFile(filePath);
            }
            else if (command == "--dir")
            {
                var directoryPath = args[1];
                var fileType = args[2];
                WriteLine($"Directory {directoryPath} selected for {fileType} files");
                ProcessDirectory(directoryPath, fileType);
            }
            else
            {
                WriteLine("Invalid Command");
            }*/          
        }

        private static void ProcessExistingFiles(string inputDirectory)
        {
            WriteLine($"Checking {inputDirectory} for existing files");

            foreach (var filePath in Directory.EnumerateFiles(inputDirectory))
            {
                WriteLine($" - Found {filePath}");
                FilesToProcess.TryAdd(filePath, filePath);
            }
        }

        private static void ProcessFiles(object stateInfo)
        {
           foreach (var fileName in FilesToProcess.Keys)
            {
                if (FilesToProcess.TryRemove(fileName, out _))
                {
                    var fileProcessor = new FileProcessor(fileName);
                    fileProcessor.Process();
                }
            }
        }

        private static void WatcherError(object sender, ErrorEventArgs e)
        {
            WriteLine($"* ERROR: File watching system may no longer be active: {e.GetException()}");
        }

        private static void FileRenamed(object sender, RenamedEventArgs e)
        {
            WriteLine($"* File renamed from {e.OldName} to {e.Name}");
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File deleted: {e.Name} - type {e.ChangeType}");
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File changed: {e.Name} - type: {e.ChangeType}");

            //var fileProcessor = new FileProcessor(e.FullPath);
            //fileProcessor.Process();

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File created: {e.Name} - type: {e.ChangeType}");

            //var fileProcessor = new FileProcessor(e.FullPath);
            //fileProcessor.Process();

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void ProcessSingleFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }

        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            switch (fileType) 
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach (var textFilePath in textFiles)
                    {
                        var fileProcessor = new FileProcessor(textFilePath);
                        fileProcessor.Process();
                    }                    
                    break;
                default:
                    WriteLine($"ERROR: {fileType} is not supported.");
                    break;

            }

            
        }
        
    }
}
