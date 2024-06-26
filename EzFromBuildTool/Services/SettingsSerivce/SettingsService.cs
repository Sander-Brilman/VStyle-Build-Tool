﻿using VStyle.Extentions;
using VStyle.Parser;
using VStyle.Services.Scanner;
using VStyle.Services.Validator;
using System.Text.Json;

namespace VStyle.Services.SettingsSerivce;
public class SettingsService
{
    public const string SettingsFileName = "VStyleBuildConfig.json";

    private readonly string _currentDirectory = Directory.GetCurrentDirectory();

    private string SettingsFilePath => Path.Combine(_currentDirectory, SettingsFileName);

    public bool IsSettingsFileAvailable()
    {
        return File.Exists(SettingsFilePath);
    }

    public async Task<CssParserSettings?> GetBuildSettingsFromFile()
    {
        using FileStream settingsFile = File.OpenRead(SettingsFilePath);
        
        try
        {
            return (await JsonSerializer.DeserializeAsync<CssParserSettings>(settingsFile))!;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("\nAn error has occoured: \n " + ex.Message);
            await Console.Out.WriteLineAsync("\n\nEither incude these fields in the settings file or delete the file and generate a new one\n\n");
            return null;
        }
    }

    public async Task GenerateExampleSettingsFile()
    {
        Console.WriteLine("\nType the path of the directory you wish to automatically include the css files of (leave empty to skip)");

        string? sourceDirectory = Console.ReadLine();

        while (string.IsNullOrEmpty(sourceDirectory) is false && Directory.Exists(sourceDirectory) is false)
        {
            Console.WriteLine("\nThe directory provided does not exist, try again or leave empty to skip");
            sourceDirectory = Console.ReadLine();
        }

        string[] cssFiles = ["C:/Path/To/CssFile.css", "C:/Path/To/Css/File/2.css"];

        if (string.IsNullOrEmpty(sourceDirectory) is false)
        {
            bool recursiveSearch = ConsoleInteractions.PromptYesNo("\nDo you also want to recursively add css files in sub-directories?\n");
            cssFiles = DirectoryScanner.FindAllFilesInDirectoryWithExtentions(sourceDirectory, ".css", recursiveSearch);
        }

        CssParserSettings settings = new()
        {
            CssFiles = cssFiles,
            OutputDirectory = "C:/Path/To/OutputDirectory",
            FinalResultFileName = "VStyleFinalResult.css",
            SaveBuildProcessSteps = false,

            CssInternalVariableOldPrefix = "internal",
            CssInternalVariableNewPrefix = "\\/",
            CssInternalVariableRandomCharacterCount = 1,

            CssNonInternalVariableNewPrefix = "\\/",
        };

        string json = JsonSerializer.Serialize(settings);

        await File.WriteAllTextAsync(SettingsFilePath, json); 
    }
}
