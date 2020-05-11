using System;
using System.IO;
using static System.Console;

namespace DataProcessor
{
    internal class FileProcessor
    {
        private static readonly string BackupDirectoryName = "backup";
        private static readonly string InProgressDirectoryName = "processing";
        private static readonly string CompletedDirectoryName = "complete";

        public string InputFilePath { get; }

        public FileProcessor(string filePath)
        {
            InputFilePath = filePath;
        }

        public void Process()
        {
            WriteLine($"Beging process of {InputFilePath}");

            if (!File.Exists(InputFilePath))
            {
                WriteLine($"ERROR: file {InputFilePath} does not exist.");
                return;
            }

            string rootDirectoryPath = new DirectoryInfo(InputFilePath).Parent.Parent.FullName;
            WriteLine($"Root directory path is {rootDirectoryPath}.");

            //checking if backup dir exists
            string inputFileDirectoryPath = Path.GetDirectoryName(InputFilePath);
            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

            if (!Directory.Exists(backupDirectoryPath))
            {
                WriteLine("Creating backup directory...");
                Directory.CreateDirectory(backupDirectoryPath);
            }

            //copy file to backup directory
            string inputFileName = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);
            WriteLine($"Coping {inputFileName} to {backupDirectoryPath}");
            File.Copy(InputFilePath, backupFilePath,true);

            //move file in processing dir
            string inProcessDirectoryPath = Path.Combine(rootDirectoryPath, InProgressDirectoryName);

            if (!Directory.Exists(inProcessDirectoryPath))
            {
                WriteLine("Creating processing directory...");
                Directory.CreateDirectory(inProcessDirectoryPath);
            }

            string inProcessFilePath = Path.Combine(inProcessDirectoryPath, inputFileName);

            if (File.Exists(inProcessFilePath))
            {
                WriteLine($"The file {inputFileName} is already being processed...");
            }

            WriteLine($"Moving the file {inputFileName} to {inProcessDirectoryPath}");
            File.Move(InputFilePath, inProcessFilePath);


            //getting file extension and moving file to completed dir
            string extension = Path.GetExtension(InputFilePath);

            switch (extension)
            {
                case ".txt":
                    ProcessTextFile(inProcessFilePath);
                    break;
                default:
                    WriteLine($"{extension} is upsupported file type.");
                    break;
            }

            string completedDirectoryPath = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
            Directory.CreateDirectory(completedDirectoryPath);

            string completedFileName = $"{Path.GetFileNameWithoutExtension(InputFilePath)} - {Guid.NewGuid()}{extension}";

            string completedFilePath = Path.Combine(completedDirectoryPath, completedFileName);

            if (!File.Exists(completedFilePath))
            {
                WriteLine($"Moving file {inputFileName} to {completedDirectoryPath}");
                File.Move(inProcessFilePath, completedFilePath);
            }

            Directory.Delete(inProcessDirectoryPath,true);
            WriteLine("<processing> directory deleted");
        }

        private void ProcessTextFile(string inProcessFilePath)
        {
            WriteLine($"Processing text file {inProcessFilePath}");
            //read and process
        }
    }
}