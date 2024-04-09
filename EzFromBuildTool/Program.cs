﻿using Cocona;
using EzFromBuildTool.Parser;
using EzFromBuildTool.Parser.Steps;
using EzFromBuildTool.Results;
using EzFromBuildTool.Services.Scanner;
using EzFromBuildTool.Services.SettingsSerivce;
using EzFromBuildTool.Services.Validator;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using OneOf.Types;
using static EzFromBuildTool.Extentions.ConsoleInteractions;
using static System.Console;


var builder = CoconaApp.CreateBuilder();

builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<SettingsValidator>();

var app = builder.Build();

app.Run(async (SettingsService settingsService, SettingsValidator validator) =>
{
    bool hasSettingsFile = settingsService.IsSettingsFileAvailable();

    if (hasSettingsFile is false)
    {
        WriteLine($"No settings file with the name {SettingsService.SettingsFileName} is in the current directory");
        bool createFile = PromptYesNo("Would you like to generate an example settings file");

        if (createFile)
        {
            await settingsService.GenerateExampleSettingsFile();
            WriteLine($"\nExample settings file with name {SettingsService.SettingsFileName} was generated in the current directory");
            WriteLine($"Edit the settings then re-run the program\n\n");
        }

        return;
    }

    WriteLine("Loading settings..");
    CssParserSettings? settings = await settingsService.GetBuildSettingsFromFile();

    if (settings is null) { return; }

    WriteLine("Validating settings..");
    OneOf<Success, ErrorMessage> result = validator.ValidateSettings(settings);

    if (result.IsT1)
    {
        WriteLine("\nError in settings: " + result.AsT1.Message);
        return;
    }

    WriteLine("\n[== Begin processing ==]\n");

    await CssParserPipeline
        .Build(settings)
        .AddStep<CssInternalVariableRenamingProccess>("InternalVariableRenaming")
        .AddStep<CssSelectorNestingProccess>("SelectorNesting")
        .AddStep<MinifyCssProccess>("Minifing")
        .RunAsync();
});