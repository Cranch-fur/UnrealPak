﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnrealPak
{
    internal class PakSettings
    {
        public string UnrealPakPath { get; }
        public string ModDirectoryPath { get; }
        public int ModIndex { get; }
        public string StartupArguments { get; }
        public string PakFilePath { get; }

        public PakSettings(string unrealPakPath, string modDirectory, int modIndex, string startupArgs)
        {
            UnrealPakPath = unrealPakPath;
            ModDirectoryPath = modDirectory;
            ModIndex = modIndex;
            StartupArguments = startupArgs;

            string modName = new DirectoryInfo(modDirectory).Name;
            PakFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Packaged", $"pakchunk{modIndex}({modName})-WindowsNoEditor.pak");
        }
    }





    internal class Program
    {
        private static string[] commandLineArguments;


        private const string engineDirectoryFile = "EngineDirectory.txt";
        private const string welcomeMessageFile = "WelcomeMessage.txt";
        private const string packagingArgumentsFile = "PackagingArguments.txt";


        private static readonly string filesListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filesList.txt");
        private const string defaultPackagingArguments = "-compress";




        private static void Exit()
        {
            Console.WriteLine("Press ENTER to exit application...");
            Console.ReadLine();

            Environment.Exit(0);
        }
        private static void ExitWithError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
            Exit();
        }




        private static void PrintWelcomeMessage()
        {
            if (!File.Exists(welcomeMessageFile)) return;

            var lines = File.ReadAllLines(welcomeMessageFile);
            Console.WriteLine(string.Join("\n", lines) + "\n");
        }




        private static string RequestEnginePath()
        {
            string enginePath;
            do
            {
                Console.WriteLine("\n[Engine Directory] Specify your Unreal Engine installation directory:");
                enginePath = Console.ReadLine();
            } while (!Directory.Exists(enginePath));


            return enginePath;
        }
        private static string GetEnginePath()
        {
            if (!File.Exists(engineDirectoryFile))
            {
                string enginePath = RequestEnginePath();

                File.WriteAllText(engineDirectoryFile, enginePath);
                Console.WriteLine($"[{engineDirectoryFile}] Path has been stored.");

                return enginePath;
            }


            return File.ReadAllText(engineDirectoryFile).Trim();
        }




        private static string GetModDirectory()
        {
            if (commandLineArguments.Length == 0)
                ExitWithError("Directory with the mod files wasn't specified through command line arguments!\nExample: UnrealPak.exe \"C:\\ModFiles\\CookieHat\"");


            string modDirectory = commandLineArguments[0].Trim('"');
            if (!Directory.Exists(modDirectory))
                ExitWithError($"Directory with the mod files doesn't exist!\n\"{modDirectory}\"");


            return modDirectory;
        }




        private static int GetModIndex()
        {
            if (commandLineArguments.Contains("-forcedIndex"))
            {
                foreach (string argument in commandLineArguments)
                {
                    if (argument.StartsWith("-index="))
                    {
                        int forcedIndex = 0;
                        if (int.TryParse(argument.Substring(7), out forcedIndex))
                            return forcedIndex;
                    }
                }


                return 0;
            }


            int modIndex;
            do
            {
                Console.Write("\n[UnrealPak] Specify index for your mod package: ");
            } while (!int.TryParse(Console.ReadLine(), out modIndex));

            return modIndex;
        }




        private static string GetUnrealPakPath(string enginePath)
        {
            string unrealPakPath = Path.Combine(enginePath, "Engine", "Binaries", "Win64", "UnrealPak.exe");
            Console.WriteLine($"[UnrealPak] Unreal Engine Directory: {enginePath}");
            Console.WriteLine($"[UnrealPak] Unreal Pak: {unrealPakPath}");


            return unrealPakPath;
        }


        private static string GetPackagingArguments()
        {
            return File.Exists(packagingArgumentsFile)
                ? File.ReadAllText(packagingArgumentsFile)
                : defaultPackagingArguments;
        }


        private static void CreateFileList(string modDirectory)
        {
            string filesList = $"\"{modDirectory}\\*.*\" \"..\\..\\..\\*.*\"";
            File.WriteAllText(filesListPath, filesList);
        }




        private static void UnrealPak(PakSettings settings)
        {
            if (!File.Exists(settings.UnrealPakPath))
                throw new FileNotFoundException($"UnrealPak.exe not found at: {settings.UnrealPakPath}");


            using (Process unrealPakProcess = new Process())
            {
                unrealPakProcess.StartInfo.FileName = settings.UnrealPakPath;
                unrealPakProcess.StartInfo.Arguments = $"\"{settings.PakFilePath}\" -create=\"{filesListPath}\" {settings.StartupArguments}";


                Console.WriteLine("\nStarting packaging process...");
                unrealPakProcess.Start();
                unrealPakProcess.WaitForExit();
            };
        }


        private static void CleanupTempFiles()
        {
            File.Delete(filesListPath);
        }




        private static void PackageMod(string enginePath, string modDirectory, int modIndex)
        {
            PakSettings pakSettings = new PakSettings(
                GetUnrealPakPath(enginePath),
                modDirectory,
                modIndex,
                GetPackagingArguments()
            );


            CreateFileList(pakSettings.ModDirectoryPath);
            UnrealPak(pakSettings);


            CleanupTempFiles();
        }




        static void Main(string[] args)
        {
            commandLineArguments = args;


            try
            {
                PrintWelcomeMessage();


                string enginePath = GetEnginePath();
                string modDirectory = GetModDirectory();


                int modIndex = GetModIndex();
                PackageMod(enginePath, modDirectory, modIndex);
            }
            catch (Exception ex)
            {
                ExitWithError(ex.Message);
            }
        }
    }
}